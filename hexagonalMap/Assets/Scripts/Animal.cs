using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public enum SpeciesType
    {
        Prey,
        Predator
    }
    
    public SpeciesType type;
    public int speed, health, vision, energy, hunger;
    public bool isBaby = false;
    public int movesUntilMating = 2;
    public int movesUntilAdult = 0;

    public Vector2Int location;
    public Vector3Int cubeLocation;
    public Vector3 worldLocation;

    public Vector2Int[] movableTiles;
    public Vector2Int[] visionTiles;
    public Vector3 moveTileWorldLoc;

    public HexGrid hexGrid = HexGrid.Instance;
    public Animator animator;
    public void SetBaseStats(int speed, int health, int vision, int energy, SpeciesType type, bool baby)
    {
        this.speed = speed;
        this.health = health;
        this.vision = vision;
        this.energy = energy;
        this.type = type;
        hunger = 0;    // 0 is full energy*10 is starving
        animator = this.gameObject.GetComponent<Animator>();
        if (baby)
        {
            movesUntilAdult = 5;
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
}
