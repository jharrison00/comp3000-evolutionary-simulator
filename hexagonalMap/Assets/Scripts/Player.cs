using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int speed, strength, health;

    public Vector2Int offsetLocation;
    public Vector3Int cubeLocation;
    private Vector3 worldLocation;
    public int moveDistance;

    public HexGrid hexGrid;
    public Transform playerPrefab;
    public Material material;
    public PlayerCursor cursor;

    private Transform player;
    private bool isPlayerSelected;
    private bool holdingPlayer;

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayer();
    }

    void Update()
    {
        cubeLocation = hexGrid.OddRToCube(offsetLocation.x, offsetLocation.y);
        if (cursor.IsPlayerSelected())
        {
            MovePlayer();
        }
        else
        {
            holdingPlayer = false;
        }
        ChangeMaterial();
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
        player.parent = this.transform;
        player.name = "PlayerModel";

        this.transform.position = worldLocation;
        SkinnedMeshRenderer skinRender = this.GetComponentInChildren<SkinnedMeshRenderer>();
        this.material = skinRender.material;    
    }

    private void SetLocation(int x, int y)
    {
        offsetLocation.x = x;
        offsetLocation.y = y;
        worldLocation = hexGrid.CalcWorldPos(offsetLocation);
        worldLocation.y = ((hexGrid.heights[x, y] * 0.1f) * 2f) + 0.2f;
    }

    private bool IsValidSpawn(int x, int y)
    {
        string hexType = hexGrid.hexType[x, y];
        return hexType != "Water";
    }

    private bool IsValidMove(string location)
    {
        if (location == "Player")
        {
            return false;
        }
        else
        { 
            return true;
        }
    }

    public void MovePlayer()
    {
        WhereToMovePlayer();
        if (offsetLocation.x < 0)
            offsetLocation.x = 0;
        else if (offsetLocation.x >= hexGrid.gridWidth)
            offsetLocation.x = hexGrid.gridWidth - 1;
        if (offsetLocation.y < 0)
            offsetLocation.y = 0;
        else if (offsetLocation.y >= hexGrid.gridHeight)
            offsetLocation.y = hexGrid.gridHeight - 1;

        SetLocation(Mathf.FloorToInt(offsetLocation.x), Mathf.FloorToInt(offsetLocation.y));
        this.transform.position = worldLocation;
    }

    private void ChangeMaterial()
    {
        SkinnedMeshRenderer skinRender = this.GetComponentInChildren<SkinnedMeshRenderer>();
        skinRender.material = this.material;
    }

    private void WhereToMovePlayer()
    {
        if (Input.GetMouseButton(0))
        {
            holdingPlayer = true;
        }
        if (holdingPlayer && !Input.GetMouseButton(0) && cursor.highlightedObject != null && IsValidMove(cursor.highlightedObject.name))
        {
            for (int x = 0; x < hexGrid.hexLocation.GetLength(0); x++)
            {
                for (int y = 0; y < hexGrid.hexLocation.GetLength(1); y++)
                {
                    string s = hexGrid.hexLocation[x, y];
                    if (s == cursor.highlightedObject.name)
                    {
                        Vector3 prevWorldLoc = worldLocation;
                        Vector3Int currLoc = hexGrid.OddRToCube(offsetLocation.x, offsetLocation.y);
                        Vector3Int moveLoc = hexGrid.OddRToCube(x, y);
                        moveDistance = hexGrid.CubeDistance(currLoc, moveLoc);
                        if (moveDistance <= 3)
                        {
                            SetLocation(x, y);
                            RotatePlayer(prevWorldLoc, worldLocation);
                        }
                        else
                        {
                            Debug.Log("Move distance too high: " + moveDistance);
                        }
                    }
                }
            }                  
        }
        if (!Input.GetMouseButton(0))
        {
            holdingPlayer = false;
        }
    }

    private void RotatePlayer(Vector3 prevLoc, Vector3 currentLoc)
    {
        float x, z;
        x = currentLoc.x - prevLoc.x;
        z = currentLoc.z - prevLoc.z;
        this.transform.rotation = Quaternion.Euler(0, 0f, 0f);
        if (x > 0 && z > 0)
            this.transform.Rotate(0, 45, 0);
        if (x > 0 && z == 0)
            this.transform.Rotate(0, 90, 0);
        if (x > 0 && z < 0)
            this.transform.Rotate(0, 135, 0);
        if (x == 0 && z < 0)
            this.transform.Rotate(0, 180, 0);
        if (x < 0 && z < 0)
            this.transform.Rotate(0, 225, 0);
        if (x < 0 && z == 0)
            this.transform.Rotate(0, 270, 0);
        if (x < 0 && z > 0)
            this.transform.Rotate(0, 315, 0);
    }
}
