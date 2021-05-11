using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public int speed, strength, vision, energy, puerperal, hunger, age, babies;
    public bool isBaby = false;

    public int movesUntilMating;
    public int movesUntilAdult = 0;
    public Animal mateCall;
    public bool eating = false;

    public Vector2Int location;
    public Vector3Int cubeLocation;
    public Vector3 worldLocation;

    public Vector2Int[] movableTiles;
    public Vector2Int[] visionTiles;
    public Vector3 moveTileWorldLoc;

    public HexGrid hexGrid = HexGrid.Instance;
    public Animator animator;

    public void SetBaseStats(int speed, int strength, int vision, int energy, int puerperal, bool baby)
    {
        this.speed = speed;
        this.strength = strength;
        this.vision = vision;
        this.energy = energy;
        this.puerperal = puerperal;

        movesUntilMating = puerperal;
        hunger = 0;    // 0 is full energy * 10 is starving
        age = 0;
        babies = 0;
        animator = this.gameObject.GetComponent<Animator>();
        if (baby)
        {
            movesUntilAdult = 4;
            this.isBaby = true;
        }
    }

    public void SetLocation(int x, int y)
    {
        location.x = x;
        location.y = y;
        worldLocation = HexGrid.Instance.CalcWorldPos(location);
        worldLocation.y = ((HexGrid.Instance.heights[x, y] * 0.1f) * 2f) + 0.2f;
        cubeLocation = hexGrid.OddRToCube(location.x, location.y);
    }

    public Vector2Int[] GetMoves(int N)
    {
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

    public int GetHexArea(int N)
    {
        int area = 1;
        for (int i = N; i >= 1; i--)
        {
            area += i * 6;
        }
        return area;
    }

    public int GetMoveDistance(string terrainType)
    {
        if (terrainType == "Grass" || terrainType == "Trees")
        {
            return speed;
        }
        else if (terrainType == "Stone" || terrainType == "Rocks")
        {
            if (speed - 1 <= 0)
            {
                return 1;
            }
            return speed - 1;
        }
        else if (terrainType == "Water")
        {
            if (speed - 2 <= 0)
            {
                return 1;
            }
            return speed - 2;
        }
        return speed;
    }

    public Vector2Int GetClosestTile(Vector2Int desiredMoveTile, Vector2Int[] tiles)
    {
        // returns closest possible tile to desired move tile
        Vector3Int cubeDest = hexGrid.OddRToCube(desiredMoveTile.x, desiredMoveTile.y);
        int minDist = 999;
        Vector2Int minTile = new Vector2Int(-1, -1);
        foreach (var tile in tiles)
        {
            Vector3Int cubeTile = hexGrid.OddRToCube(tile.x, tile.y);
            int dist = hexGrid.CubeDistance(cubeTile, cubeDest);
            if (dist < minDist)
            {
                minDist = dist;
                minTile = tile;
            }
        }
        return minTile;
    }

    public Vector2Int GetFurthestTile(Vector2Int desiredMoveTile, Vector2Int[] tiles)
    {
        // returns closest possible tile to desired move tile
        Vector3Int cubeDest = hexGrid.OddRToCube(desiredMoveTile.x, desiredMoveTile.y);
        int maxDist = 0;
        int maxWaterDist = 0;
        Vector2Int maxTile = new Vector2Int(-1, -1);
        Vector2Int maxWaterTile = new Vector2Int(-1, -1);
        foreach (var tile in tiles)
        {
            string terrainType = hexGrid.hexType[tile.x, tile.y];
            if (terrainType != "Water")
            {
                Vector3Int cubeTile = hexGrid.OddRToCube(tile.x, tile.y);
                int dist = hexGrid.CubeDistance(cubeTile, cubeDest);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    maxTile = tile;
                }
            }
            else
            {
                Vector3Int cubeTile = hexGrid.OddRToCube(tile.x, tile.y);
                int dist = hexGrid.CubeDistance(cubeTile, cubeDest);
                if (dist > maxDist)
                {
                    maxWaterDist = dist;
                    maxWaterTile = tile;
                }
            }
        }
        if (maxTile == new Vector2Int(-1, -1))
        {
            return maxWaterTile;
        }

        return maxTile;
    }

    public void Move(Vector2Int moveTile)
    {
        Vector3 tileWorldLoc;
        tileWorldLoc = HexGrid.Instance.CalcWorldPos(moveTile);
        tileWorldLoc.y = ((HexGrid.Instance.heights[moveTile.x, moveTile.y] * 0.1f) * 2f) + 0.2f;
        moveTileWorldLoc = tileWorldLoc;
        transform.LookAt(new Vector3(tileWorldLoc.x, transform.position.y, tileWorldLoc.z));
        location.x = moveTile.x;
        location.y = moveTile.y;
        cubeLocation = hexGrid.OddRToCube(location.x, location.y);
        hunger += 10;
    }

    public Vector2Int TryToAvoidBadTerrain(Vector2Int[] tiles)
    {
        Vector2Int tile = tiles[Random.Range(0, tiles.Length)];
        for (int i = 0; i < 5; i++)
        {
            string terrainType = hexGrid.hexType[tile.x, tile.y];
            if (terrainType != "Water" && tile != location)
            {
                return tile;
            }
            tile = tiles[Random.Range(0, tiles.Length)];
        }
        return tile;
    }

    public bool CheckMatingCalls()
    {
        if (mateCall != null)
        {
                return true;
        }
        return false;
    }

}
