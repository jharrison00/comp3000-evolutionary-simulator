using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : Animal
{
    public ChickensController chickensController = ChickensController.Instance;

    private bool eating = false;
    private bool timerActive = false;
    private float timer = 0;

    public Chicken mateCallSent;
    public Chicken mateCallRecieved;

    public void Update()
    {
        bool moving = false;
        if (timerActive)
        {
            timer += Time.deltaTime;
            if (timer >= 2)
            {
                timerActive = false;
                timer = 0;
            }
        }
        if (moveTileWorldLoc != new Vector3(0, 0, 0))
        {
            if (Vector3.Distance(moveTileWorldLoc, worldLocation) > 0.1f)
            {
                // Character is moving
                animator.SetBool("Walk", true);
                Vector3 dist = moveTileWorldLoc - worldLocation;
                transform.localPosition += dist * Time.deltaTime;
                moving = true;
            }
            if (Vector3.Distance(moveTileWorldLoc, worldLocation) < 0.1f)
            {
                // Character has moved
                transform.localPosition = moveTileWorldLoc;
                moveTileWorldLoc = new Vector3(0, 0, 0);
                animator.SetBool("Walk", false);
                if (eating)
                {
                    // If move is done but character is eating (start a timer)
                    timerActive = true;
                }
                moving = false;
            }
            worldLocation = transform.localPosition;
        }
        if (eating)
            Eat();
        if (hunger >= energy * 10) 
            chickensController.Kill(this);
        if (!timerActive && !moving)
        {
            if (movesUntilAdult == 0 && isBaby)
            {
                chickensController.GrowUp(this);
            }
            // Do next move when there is no timer and not already moving
            ChooseMove();
        }
    }

    public void ChooseMove()
    {
        animator.SetBool("Eat", false);
        // Get surrounding info (nearby food + chickens)
        string currentLocationType = hexGrid.hexType[location.x, location.y];
        movableTiles = GetMoves(GetMoveDistance(currentLocationType));
        Vector2Int[] movableTilesExcludingOthers = RemoveChickens(movableTiles);
        visionTiles = GetMoves(vision);
        Vector2Int foodTile = NearestFood(movableTilesExcludingOthers);
        Vector2Int foodInVision = NearestFood(visionTiles);
        Vector2Int chickenInVision = chickensController.IsChickenNear(visionTiles, this);
        Vector2Int none = new Vector2Int(-1, -1);
        Vector2Int moveTile = new Vector2Int();

        bool reproducing = CheckMatingCalls();

        // If going to mate = move to mating chicken
        if (reproducing)
        {
            moveTile = GetClosestTile(mateCallSent.location, movableTiles);
            int distance = hexGrid.CubeDistance(hexGrid.OddRToCube(moveTile.x, moveTile.y), hexGrid.OddRToCube(mateCallSent.location.x, mateCallSent.location.y));
            if (distance == 0)
            {
                chickensController.CreateBaby(this, mateCallSent, moveTile);
            }
        }
        // If there is food and is hungry = go to food source
        else if (foodInVision != none && hunger >= 40)
        {
            moveTile = GetClosestTile(foodInVision, movableTilesExcludingOthers);
            if (hexGrid.CubeDistance(hexGrid.OddRToCube(foodTile.x, foodTile.y), hexGrid.OddRToCube(moveTile.x, moveTile.y)) == 0) 
            {
                eating = true;
                hunger = 0;
            }
        }
        // If there is no food but is hungry = go to food if any in vision
        else if (foodTile == none && hunger >= 40 && foodInVision != none)
        {
            moveTile = GetClosestTile(foodInVision, movableTilesExcludingOthers);
        }
        // If there is another chicken and not hungry = send signal to reproduce and stay still
        else if (chickenInVision != none && hunger <= 40 && movesUntilMating == 0)
        {
            chickenInVision = chickensController.IsChickenNear(visionTiles, this);
            Chicken chicken = chickensController.GetChickenAtLocation(chickenInVision);
            chickensController.SendMateSignal(this, chicken);
            moveTile = this.location;
        }
        // Else make a random move (attempt to avoid water + stone)
        else
        {
            moveTile = TryToAvoidBadTerrain(movableTilesExcludingOthers);
        }

        this.hunger += 10;   // increase hunger each move
        movesUntilMating--;
        if (movesUntilMating<0)
            movesUntilMating = 0;
        if (movesUntilAdult != 0 && isBaby)
            movesUntilAdult -= 1;

        Move(moveTile);

    }

    private bool CheckMatingCalls()
    {
        if (mateCallSent != null && mateCallRecieved != null)
        {
            if (mateCallSent == mateCallRecieved)
            {
                return true;
            }
        }
        return false;
    }

    private Vector2Int GetClosestTile(Vector2Int destinationTile, Vector2Int[] tiles)
    {
        Vector3Int cubeDest = hexGrid.OddRToCube(destinationTile.x, destinationTile.y);
        int minDist = 999;
        Vector2Int minTile = new Vector2Int(-1, -1);
        foreach (var tile in tiles)
        {
            Vector3Int cubeTile =  hexGrid.OddRToCube(tile.x, tile.y);
            int dist = hexGrid.CubeDistance(cubeTile, cubeDest);
            if (dist < minDist)
            {
                minDist = dist;
                minTile = tile;
            }
        }
        return minTile;
    }

    private Vector2Int[] GetMoves(int N)
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

    private Vector2Int NearestFood(Vector2Int[] searchTiles)
    {
        Vector2Int[] foodTiles = new Vector2Int[searchTiles.Length];
        int[] distances = new int[searchTiles.Length];
        int c = 0;

        foreach (Vector2Int tile in searchTiles)
        {
            string tileType = hexGrid.hexType[tile.x, tile.y];
            if (tileType == "Vegetation")
            {
                // Convert tile to cube location and calc distance from current tile
                Vector3Int cubeTile = hexGrid.OddRToCube(tile.x, tile.y);
                distances[c] = hexGrid.CubeDistance(cubeLocation, cubeTile);
                foodTiles[c] = tile;
                c++;
            }
        }      

        int minDist = 999;
        Vector2Int minTile = new Vector2Int(-1, -1);
        for (int i = 0; i < c; i++)
        {
            if (distances[i] < minDist)
            {
                minDist = distances[i];
                minTile = foodTiles[i];
            }
        }
        return minTile;
    }

    private int GetMoveDistance(string terrainType)
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
            if (speed -2 <= 0)
            {
                return 1;
            }
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
        Vector2Int tile = tiles[UnityEngine.Random.Range(0, tiles.Length)];
        for (int i = 0; i < 5; i++)
        {
            string terrainType = hexGrid.hexType[tile.x, tile.y];
            if (terrainType != "Water" && tile != location)
            {
                return tile;
            }
            tile = tiles[UnityEngine.Random.Range(0, tiles.Length)];
        }
        return tile;
    }

    private void Eat()
    {
        if (moveTileWorldLoc == new Vector3(0, 0, 0))
        {
            animator.SetBool("Eat", true);
            this.hunger = 0;
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


    private Vector2Int[] RemoveChickens(Vector2Int[] tiles)
    {
        Vector2Int[] newTiles = new Vector2Int[tiles.Length];
        int c = 0;
        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2Int tile = tiles[i];
            Chicken chicken = chickensController.GetChickenAtLocation(tile);
            if (chicken == null || chicken.location == this.location) 
            {
                newTiles[c] = tile;
                c++;
            }
        }
        Array.Resize(ref newTiles, c);

        return newTiles;
    }
    
}
