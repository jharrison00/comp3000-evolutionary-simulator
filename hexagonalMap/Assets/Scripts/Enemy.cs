using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int speed, strength, health, hunger;
    public Vector2Int location;
    public Vector3Int cubeLocation;
    public Vector3 worldLocation;

    public Vector2Int[] movableTiles;
    public HexGrid hexGrid = HexGrid.Instance;
    public EnemiesController enemiesController = EnemiesController.Instance;
    public Animator animator;
    public Vector3 moveTileWorldLoc;
    public bool eating = false;

    public void Update()
    {
        if (moveTileWorldLoc != new Vector3(0,0,0)) 
        {
            if (Vector3.Distance(moveTileWorldLoc, worldLocation) > 0.2f)
            {
                animator.SetBool("Walk", true);
                Vector3 dist = moveTileWorldLoc - worldLocation;
                transform.localPosition += dist * Time.deltaTime;
            }
            if (Vector3.Distance(moveTileWorldLoc, worldLocation) < 0.2f)
            {
                transform.localPosition = moveTileWorldLoc;
                moveTileWorldLoc = new Vector3(0, 0, 0);
                animator.SetBool("Walk", false);

            }
            worldLocation = transform.localPosition;
        }
        if (eating)
            Eat();
        if (hunger >= 100)
        {
            enemiesController.Kill(this);
        }
    }

    public void SetBaseStats()
    {
        speed = 3;
        strength = 3;
        health = 3;
        hunger = 0;   // 0 is good 100 is starving
        animator = this.gameObject.GetComponent<Animator>();
    }

    public void SetLocation(int x, int y)
    {
        location.x = x;
        location.y = y;
        worldLocation = HexGrid.Instance.CalcWorldPos(location);
        worldLocation.y = ((HexGrid.Instance.heights[x, y] * 0.1f) * 2f) + 0.2f;
        cubeLocation = hexGrid.OddRToCube(location.x, location.y);
    }

    public void ChooseMove()
    {
        animator.SetBool("Eat", false);
        // Get surrounding info ( nearby food + enemies)
        movableTiles = GetMoves();
        Vector2Int foodTile = IsFood();
        Vector2Int enemyTile = enemiesController.IsPlayerNear(movableTiles, this);
        Vector2Int none = new Vector2Int(-1, -1);

        Vector2Int moveTile = new Vector2Int();
        // If there is food and is hungry = go to food source
        if (foodTile != none && hunger >= 40) 
        {
            moveTile = foodTile;
            eating = true;
        }
        // If there is food and enemy but player is really hungry = go to food source (risky)
        else if (foodTile != none && enemyTile != none && hunger >= 70) 
        {
            moveTile = foodTile;
            eating = true;
        }
        else
        {
            moveTile = TryToAvoidBadTerrain(movableTiles);
        }
        hunger += 10;   // increase hunger each move

        Move(moveTile);
    }

    private Vector2Int[] GetMoves()
    {
        string currentLocationType = hexGrid.hexType[location.x, location.y];
        int N = GetMoveDistance(currentLocationType);
        int c = 0;
        int area = GetHexArea(N);
        Vector2Int[] tiles = new Vector2Int[area];
        for (int x = -N; x <= N; x++)
        {
            for (int y = -N; y <= N; y++)
            {
                for (int z = -N; z <= N; z++)
                {
                    if (x + y + z == 0)
                    {
                        Vector2Int tile = hexGrid.CubeToOddR(cubeLocation.x + x, cubeLocation.y + y, cubeLocation.z + z);
                        if (tile.x >= 0 && tile.x <= hexGrid.gridWidth - 1 && tile.y >= 0 && tile.y <= hexGrid.gridHeight - 1)
                        {
                            tiles[c] = tile;
                            c++;
                        }
                    }
                }
            }
        }
        if (c != area)
        {
            Vector2Int[] shapedTiles = new Vector2Int[c];   // resize array
            for (int i = 0; i < c; i++)
            {
                shapedTiles[i] = tiles[i];
            }
            return shapedTiles;
        }
        else
        {
            return tiles;
        }
    }

    private Vector2Int IsFood()
    {
        foreach (Vector2Int tile in movableTiles)
        {
            string tileType = hexGrid.hexType[tile.x, tile.y];
            if (tileType == "Vegetation")
            {
                return tile;
            }
        }
        return new Vector2Int(-1, -1);
    }

    private int GetMoveDistance(string terrainType)
    {
        if (terrainType == "Grass" || terrainType == "Trees")
        {
            return speed;
        }
        else if (terrainType == "Stone" || terrainType == "Rocks")
        {
            return speed - 1;
        }
        else if (terrainType == "Water")
        {
            return speed - 2;
        }
        return speed;
    }

    private int GetHexArea(int N)
    {
        int area = 1;
        for (int i = N; i >= 1; i--)
        {
            area += i * 6;
        }
        return area;
    }

    private Vector2Int TryToAvoidBadTerrain(Vector2Int[] tiles)
    {
        Vector2Int tile = tiles[Random.Range(0, tiles.Length)];
        for (int i = 0; i < 5; i++)
        {
            string terrainType = hexGrid.hexType[tile.x, tile.y];
            if (terrainType != "Water" && terrainType != "Stone" && terrainType != "Rocks" && tile != location)
            {
                return tile;
            }
            tile = tiles[Random.Range(0, tiles.Length)];
        }
        return tile;
    }

    private void Eat()
    {
        if (moveTileWorldLoc == new Vector3(0, 0, 0))
        {
            animator.SetBool("Eat", true);
            hunger = 0;
            Debug.Log(this.name + " has eaten");
            hexGrid.RespawnFood(location);
            eating = false;
        }
    }

    private void Move(Vector2Int moveTile)
    {
        Vector3 tileWorldLoc;
        tileWorldLoc = HexGrid.Instance.CalcWorldPos(moveTile);
        tileWorldLoc.y = ((HexGrid.Instance.heights[moveTile.x, moveTile.y] * 0.1f) * 2f) + 0.2f;
        moveTileWorldLoc = tileWorldLoc;
        transform.LookAt(new Vector3(tileWorldLoc.x, transform.position.y, tileWorldLoc.z));
        location.x = moveTile.x;
        location.y = moveTile.y;
        cubeLocation = hexGrid.OddRToCube(location.x, location.y);
    }

}
