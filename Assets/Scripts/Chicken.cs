using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : Animal
{
    public ChickensController chickensController = ChickensController.Instance;

    private bool timerActive = false;
    private float timer = 0;

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
        {
            if (isBaby)
            {
                chickensController.KillBaby(this);
            }
            else
            {
                chickensController.Kill(this);
            }
            chickensController.totalDeaths++;
            chickensController.starved++;
        }
        if (!timerActive && !moving)
        {
            if (movesUntilAdult == 0 && isBaby)
            {
                chickensController.GrowUp(this);
            }
            // Do next move when there is no timer and not already moving
            ChooseMove();
            age++;
        }
    }

    public void ChooseMove()
    {
        animator.SetBool("Eat", false);
        // Get surrounding info (nearby food + chickens)
        string currentLocationType = hexGrid.hexType[location.x, location.y];       
        movableTiles = GetMoves(GetMoveDistance(currentLocationType));                      // gets all tiles within object's speed and terrain type
        Vector2Int[] movableTilesExcludingOthers = RemoveChickens(movableTiles);            // all movable tiles that don't contain chickens
        visionTiles = GetMoves(vision);                                                     // gets all tiles within object's vision
        Vector2Int foodTile = NearestFood(movableTilesExcludingOthers);                     // gets closest food to object
        Vector2Int foodInVision = NearestFood(visionTiles);                                 // gets closest food in object's vision
        Vector2Int chickenInVision = chickensController.IsChickenNear(visionTiles, this);   // gets closest chicken in object's vision (for mating)
        Vector2Int nearestWolf = NearestWolf(visionTiles);                                  // gets closest wolf in object's vision (for running away)
        Vector2Int none = new Vector2Int(-1, -1);                                           
        Vector2Int moveTile = new Vector2Int();

        bool reproducing = CheckMatingCalls();              // if mate call sent and recieved are equal - trigger reproduction

        // If going to mate = move to mating chicken
        if (reproducing)
        {
            moveTile = GetClosestTile(mateCall.location, movableTiles);
            int distance = hexGrid.CubeDistance(hexGrid.OddRToCube(moveTile.x, moveTile.y), hexGrid.OddRToCube(mateCall.location.x, mateCall.location.y));
            if (distance == 0 && movesUntilMating == 0 && mateCall.movesUntilMating == 0) 
            {
                // Get random number for amount of babies
                int numBabies = UnityEngine.Random.Range(1, 3);
                babies += numBabies;
                mateCall.babies += numBabies;
                Debug.Log(this.name + " and " + mateCall.name + " had " + numBabies + " babies");
                for (int i = 0; i < numBabies; i++)
                {
                    chickensController.CreateBaby(this, (Chicken)mateCall, moveTile);
                }
                hunger += 30;
                mateCall.hunger += 30;
                mateCall.mateCall = null;
                mateCall = null;
            }
        }
        //If there is a wolf in vision and not near death = run as far away as possible (only from closest wolf)
        else if (nearestWolf != none && hunger <= 60) 
        {
            moveTile = GetFurthestTile(nearestWolf, movableTilesExcludingOthers);
        }
        // If there is food and is hungry = go to food source
        else if (foodInVision != none && hunger >= 40)
        {
            moveTile = GetClosestTile(foodInVision, movableTilesExcludingOthers);
            if (hexGrid.CubeDistance(hexGrid.OddRToCube(foodInVision.x, foodInVision.y), hexGrid.OddRToCube(moveTile.x, moveTile.y)) == 0) 
            {
                eating = true;
                hunger = 0;
            }
        }
        // If there is no food but is hungry = go as close to food if any in vision
        else if (foodTile == none && hunger >= 40 && foodInVision != none)
        {
            moveTile = GetClosestTile(foodInVision, movableTilesExcludingOthers);
        }
        // Else make a random move (attempt to avoid water)
        else
        {
            moveTile = TryToAvoidBadTerrain(movableTilesExcludingOthers);
        }
        // If there is another chicken and not hungry = send signal to reproduce
        if (chickenInVision != none && hunger <= 40 && movesUntilMating == 0 && !isBaby)
        {
            chickenInVision = chickensController.IsChickenNear(visionTiles, this);
            Chicken chicken = chickensController.GetChickenAtLocation(chickenInVision, true);
            chickensController.SendMateSignal(this, chicken);
        }
        movesUntilMating--;
        if (movesUntilMating < 0)
            movesUntilMating = 0;
        if (movesUntilAdult != 0 && isBaby)
            movesUntilAdult -= 1;
        if (moveTile == none)
        {
            moveTile = this.location;
        }
        Move(moveTile);
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

    private Vector2Int[] RemoveChickens(Vector2Int[] tiles)
    {
        Vector2Int[] newTiles = new Vector2Int[tiles.Length];
        int c = 0;
        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2Int tile = tiles[i];
            Chicken chicken = chickensController.GetChickenAtLocation(tile, false);
            if (chicken == null || chicken.location == this.location) 
            {
                newTiles[c] = tile;
                c++;
            }
        }
        Array.Resize(ref newTiles, c);

        return newTiles;
    }

    public void Eat()
    {
        if (moveTileWorldLoc == new Vector3(0, 0, 0))
        {
            animator.SetBool("Eat", true);
            this.hunger = 0;
            hexGrid.RespawnFood(location);
            eating = false;
        }
    }

    private Vector2Int NearestWolf(Vector2Int[] searchTiles)
    {
        Vector2Int[] wolfTiles = new Vector2Int[searchTiles.Length];
        int[] distances = new int[searchTiles.Length];
        int c = 0;

        foreach (Vector2Int tile in searchTiles)
        {
            foreach (var wolf in WolvesController.Instance.animals)
            {
                if (wolf.location == tile)
                {
                    // Convert tile to cube location and calc distance from current tile
                    Vector3Int cubeTile = hexGrid.OddRToCube(tile.x, tile.y);
                    distances[c] = hexGrid.CubeDistance(cubeLocation, cubeTile);
                    wolfTiles[c] = tile;
                    c++;
                }
            }
        }
        int minDist = 999;
        Vector2Int minTile = new Vector2Int(-1, -1);
        for (int i = 0; i < c; i++)
        {
            if (distances[i] < minDist)
            {
                minDist = distances[i];
                minTile = wolfTiles[i];
            }
        }
        return minTile;
    }
}
