using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Animal
{
    public WolvesController wolvesController = WolvesController.Instance;

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
        if (hunger >= energy * 10)
            wolvesController.Kill(this);
        if (!timerActive && !moving)
        {
            if (movesUntilAdult == 0 && isBaby)
            {
                wolvesController.GrowUp(this);
            }
            // Do next move when there is no timer and not already moving
            ChooseMove();
        }
    }

    public void ChooseMove()
    {
        animator.SetBool("Eat", false);
        // Get surrounding info (nearby food + chickens + wolves)
        string currentLocationType = hexGrid.hexType[location.x, location.y];
        movableTiles = GetMoves(GetMoveDistance(currentLocationType));              // gets all tiles within object's speed and terrain type
        Vector2Int[] movableTilesExcludingOthers = RemoveWolves(movableTiles);      // all movable tiles that don't contain wolves
        visionTiles = GetMoves(vision);                                             // gets all tiles within object's vision
        Vector2Int foodTile = NearestChicken(movableTilesExcludingOthers);          // gets closest chicken to object
        Vector2Int foodInVision = NearestChicken(visionTiles);                      // gets closest chicken in object's vision
        Vector2Int wolfInVision = wolvesController.IsWolfNear(visionTiles, this);   // gets closest wolf in object's vision (for mating)
        Vector2Int none = new Vector2Int(-1, -1);
        Vector2Int moveTile = new Vector2Int();

        bool reproducing = CheckMatingCalls();  // if mate call sent and recieved are equal - trigger reproduction

        // If going to mate = move to mating chicken
        if (reproducing)
        {
            moveTile = GetClosestTile(mateCallSent.location, movableTiles);
            int distance = hexGrid.CubeDistance(hexGrid.OddRToCube(moveTile.x, moveTile.y), hexGrid.OddRToCube(mateCallSent.location.x, mateCallSent.location.y));
            if (distance == 0)
            {
                wolvesController.CreateBaby(this, (Wolf)mateCallSent, moveTile);
            }
        }
        // If there is food and is hungry = go to food source
        else if (foodInVision != none && hunger >= 40)
        {
            // get as close as possible to food that object can see
            moveTile = GetClosestTile(foodInVision, movableTilesExcludingOthers);  
            if (hexGrid.CubeDistance(hexGrid.OddRToCube(foodInVision.x, foodInVision.y), hexGrid.OddRToCube(moveTile.x, moveTile.y)) == 0)
            {
                // if object has moved onto the tile successfully
                Chicken chicken = wolvesController.GetChickenAtLocation(foodInVision);     // gets chicken object 
                eating = true;
                Eat(chicken);
            }
        }        
        // If there is no food but is hungry = go as close to food if any in vision
        else if (foodTile == none && hunger >= 40 && foodInVision != none)
        {
            moveTile = GetClosestTile(foodInVision, movableTilesExcludingOthers);
        }        
        // If there is another wolf and not hungry = send signal to reproduce and stay still
        else if (wolfInVision != none && hunger <= 40 && movesUntilMating == 0)
        {
            wolfInVision = wolvesController.IsWolfNear(visionTiles, this);  // secondary check to make sure there is still a wolf near
            Wolf wolf = wolvesController.GetWolfAtLocation(wolfInVision);
            wolvesController.SendMateSignal(this, wolf);
            moveTile = this.location;
        }
        // Else make a random move (attempt to avoid water)
        else
        {
            moveTile = TryToAvoidBadTerrain(movableTilesExcludingOthers);
        }

        this.hunger += 10;   // increase hunger each move
        movesUntilMating--;
        if (movesUntilMating < 0)
            movesUntilMating = 0;
        if (movesUntilAdult != 0 && isBaby)
            movesUntilAdult -= 1;

        Move(moveTile);
    }

    private Vector2Int NearestChicken(Vector2Int[] searchTiles)
    {
        Vector2Int[] chickenTiles = new Vector2Int[searchTiles.Length];
        int[] distances = new int[searchTiles.Length];
        int c = 0;

        foreach (Vector2Int tile in searchTiles)
        {
            foreach (var chicken in ChickensController.Instance.animals) 
            {
                if (chicken.location == tile)
                {
                    // Convert tile to cube location and calc distance from current tile
                    Vector3Int cubeTile = hexGrid.OddRToCube(tile.x, tile.y);
                    distances[c] = hexGrid.CubeDistance(cubeLocation, cubeTile);
                    chickenTiles[c] = tile;
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
                minTile = chickenTiles[i];
            }
        }
        return minTile;
    }

    private Vector2Int[] RemoveWolves(Vector2Int[] tiles)
    {
        Vector2Int[] newTiles = new Vector2Int[tiles.Length];
        int c = 0;
        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2Int tile = tiles[i];
            Wolf wolf = wolvesController.GetWolfAtLocation(tile);
            if (wolf == null || wolf.location == this.location)
            {
                newTiles[c] = tile;
                c++;
            }
        }
        Array.Resize(ref newTiles, c);
        return newTiles;
    }

    public void Eat(Chicken chickenToEat)
    {
        animator.SetBool("Eat", true);
        wolvesController.EatChicken(chickenToEat, this);
        chickenToEat = null;
        hunger = 0;
        eating = false;
    }
}
