using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int speed, strength, health;
    public Vector2Int location;
    public Vector3 worldLocation;

    public void DoFunction()
    {
        speed = 5;
        strength = 5;
        health = 5;
    }

    public void SetLocation(int x, int y)
    {
        location.x = x;
        location.y = y;
        worldLocation = HexGrid.Instance.CalcWorldPos(location);
        worldLocation.y = ((HexGrid.Instance.heights[x, y] * 0.1f) * 2f) + 0.2f;
    }

}
