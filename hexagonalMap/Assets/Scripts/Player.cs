using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int speed, strength, health;
    public Vector2 location;
    private Vector3 worldLocation;
    public HexGrid hexGrid;
    public Transform playerPrefab;
    public Transform player;

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayer();
    }

    void Update()
    {
        MovePlayer();
    }

    void SpawnPlayer()
    {
        int x = UnityEngine.Random.Range(0, hexGrid.gridWidth);
        int y = UnityEngine.Random.Range(0, hexGrid.gridHeight);

        SetLocation(x, y);

        if (!IsValidSpawn(x, y))
        {
            x = UnityEngine.Random.Range(0, hexGrid.gridWidth);
            y = UnityEngine.Random.Range(0, hexGrid.gridHeight);
            SetLocation(x, y);
        }

        player = Instantiate(playerPrefab) as Transform;
        player.position = worldLocation;
        player.parent = this.transform;
        player.name = "PlayerModel";
        this.transform.position = player.position;
    }

    void SetLocation(int x, int y)
    {
        location.x = x;
        location.y = y;

        worldLocation = hexGrid.CalcWorldPos(location);
        worldLocation.y = ((hexGrid.heights[x, y] * 0.1f) * 2f) + 0.2f;
    }

    bool IsValidSpawn(int x, int y)
    {
        string hexType = hexGrid.hexType[x, y];
        return hexType != "Water";
    }

    void MovePlayer()
    {
        if (location.x < 0)
            location.x = 0;
        else if (location.x >= hexGrid.gridWidth)
            location.x = hexGrid.gridWidth - 1;
        if (location.y < 0)
            location.y = 0;
        else if (location.y >= hexGrid.gridHeight)
            location.y = hexGrid.gridHeight - 1;

        SetLocation(Mathf.FloorToInt(location.x), Mathf.FloorToInt(location.y));
        player.position = worldLocation;
    }
}
