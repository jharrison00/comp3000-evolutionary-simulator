using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Animal
{
    public WolvesController wolvesController = WolvesController.Instance;

    public void Update()
    {
        bool moving = false;
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
                moving = false;
            }
            worldLocation = transform.localPosition;
        }
        if (hunger >= energy * 10)
        {
            wolvesController.Kill(this);
            wolvesController.totalDeaths++;
            Debug.Log(name + " has died to hunger");
        }
        if (!moving)
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
        eating = false;
        // Get surrounding info (nearby food + chickens + wolves)
        string currentLocationType = hexGrid.hexType[location.x, location.y];
        movableTiles = GetMoves(GetMoveDistance(currentLocationType));              // gets all tiles within object's speed and terrain type
        Vector2Int[] movableTilesExcludingOthers = RemoveWolves(movableTiles);      // all movable tiles that don't contain wolves
        visionTiles = GetMoves(vision);                                             // gets all tiles within object's vision
        Vector2Int foodTile = NearestEatableChicken(movableTilesExcludingOthers);          // gets closest chicken to object
        Vector2Int foodInVision = NearestEatableChicken(visionTiles);                      // gets closest chicken in object's vision
        Vector2Int wolfInVision = wolvesController.IsWolfNear(visionTiles, this);   // gets closest wolf in object's vision (for mating)
        Vector2Int none = new Vector2Int(-1, -1);
        Vector2Int moveTile = new Vector2Int();
        Chicken chickenToEat = null;


        bool reproducing = CheckMatingCalls();  // if mate call sent and recieved are equal - trigger reproduction

        // If going to mate = move to mating chicken
        if (reproducing)
        {
            moveTile = GetClosestTile(mateCall.location, movableTiles);
            int distance = hexGrid.CubeDistance(hexGrid.OddRToCube(moveTile.x, moveTile.y), hexGrid.OddRToCube(mateCall.location.x, mateCall.location.y));
            if (distance == 0 && movesUntilMating == 0 && mateCall.movesUntilMating == 0)
            {
                // Get random number for amount of babies
                int numBabies = UnityEngine.Random.Range(1, 3);
                Debug.Log(this.name + " and " + mateCall.name + " had " + numBabies + " babies");
                for (int i = 0; i < numBabies; i++)
                {
                    wolvesController.CreateBaby(this, (Wolf)mateCall, moveTile);
                }
                hunger += 30;
                mateCall.hunger += 30;
                mateCall.mateCall = null;
                mateCall = null;
            }
        }
        // If there is food and is hungry = go to food source
        else if (foodInVision != none && hunger >= 50)
        {
            // get as close as possible to food that object can see
            moveTile = GetClosestTile(foodInVision, movableTilesExcludingOthers);  
            if (hexGrid.CubeDistance(hexGrid.OddRToCube(foodInVision.x, foodInVision.y), hexGrid.OddRToCube(moveTile.x, moveTile.y)) == 0)
            {
                // if object has moved onto the tile successfully
                chickenToEat = wolvesController.GetChickenAtLocation(foodInVision);     // gets chicken object 
                eating = true;
            }
        }        
        // If there is no food but is hungry = go as close to food if any in vision
        else if (foodTile == none && hunger >= 50 && foodInVision != none)
        {
            moveTile = GetClosestTile(foodInVision, movableTilesExcludingOthers);
        }        
        // Else make a random move (attempt to avoid water)
        else
        {
            moveTile = TryToAvoidBadTerrain(movableTilesExcludingOthers);
        }
        // If there is another wolf and not hungry = send signal to reproduce
        if (wolfInVision != none && hunger <= 40 && movesUntilMating == 0)
        {
            wolfInVision = wolvesController.IsWolfNear(visionTiles, this);  // secondary check to make sure there is still a wolf near
            Wolf wolf = wolvesController.GetWolfAtLocation(wolfInVision);
            wolvesController.SendMateSignal(this, wolf);
        }

        movesUntilMating--;
        if (movesUntilMating < 0)
            movesUntilMating = 0;
        if (movesUntilAdult != 0 && isBaby)
            movesUntilAdult -= 1;

        Move(moveTile);
        if (eating == true && chickenToEat != null) 
        {
            Eat(chickenToEat);
        }
    }

    private Vector2Int NearestEatableChicken(Vector2Int[] searchTiles)
    {
        Vector2Int[] chickenTiles = new Vector2Int[searchTiles.Length];
        int[] distances = new int[searchTiles.Length];
        Chicken[] chickens = new Chicken[searchTiles.Length];
        int c = 0;

        foreach (Vector2Int tile in searchTiles)
        {
            foreach (var chicken in ChickensController.Instance.animals) 
            {
                if (chicken.location == tile)
                {
                    if (chicken.strength <= strength)
                    {
                        // only add if chicken is eatable
                        // Convert tile to cube location and calc distance from current tile
                        Vector3Int cubeTile = hexGrid.OddRToCube(tile.x, tile.y);
                        if (c >=distances.Length)
                        {
                            break;
                        }

                        distances[c] = hexGrid.CubeDistance(cubeLocation, cubeTile);
                        chickenTiles[c] = tile;
                        chickens[c] = (Chicken)chicken;
                        c++;
                    }
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
        // if the chicken is stronger than the attack -> the attack will not work
        if (chickenToEat.strength <= this.strength) 
        {
            animator.SetBool("Eat", true);
            wolvesController.EatChicken(chickenToEat, this);
            chickenToEat = null;
            hunger = 0;
        }
    }
}
