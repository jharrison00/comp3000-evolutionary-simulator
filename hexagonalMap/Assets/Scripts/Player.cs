using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int speed, strength, health;

    public Vector2Int location;
    public Vector3Int cubeLocation;
    private Vector3 worldLocation;
    public int moveDistance;

    private HexGrid hexGrid;
    public Transform playerPrefab;
    public Material material;
    public PlayerCursor cursor;

    private Transform player;
    public bool holdingPlayer;
    private bool isGridHighlighted = false;
    public Vector2Int[] highlightedTiles;

    // Start is called before the first frame update
    void Start()
    {
        hexGrid = HexGrid.Instance;
        SpawnPlayer();
        speed = 3;
    }

    void Update()
    {
        cubeLocation = hexGrid.OddRToCube(location.x, location.y);
        if (cursor.IsPlayerSelected())
        {
            if (!isGridHighlighted)
                HighlightGrid();
            MovePlayer();
        }
        else
        {
            if (highlightedTiles != null)
            {
                UnhighlightGrid(highlightedTiles);
                isGridHighlighted = false;
            }
            holdingPlayer = false;
        }
        ChangeMaterial();
    }
    void SpawnPlayer()
    {
        int x = UnityEngine.Random.Range(0, hexGrid.gridWidth);
        int y = UnityEngine.Random.Range(0, hexGrid.gridHeight);

        SetLocation(x, y);
        int i = 0;
        while (!IsValidSpawn(x, y))
        {
            if (i > 50)
                break;
            x = UnityEngine.Random.Range(0, hexGrid.gridWidth);
            y = UnityEngine.Random.Range(0, hexGrid.gridHeight);
            SetLocation(x, y);
            i++;
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
        location.x = x;
        location.y = y;
        worldLocation = hexGrid.CalcWorldPos(location);
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
        if (location.x < 0)
            location.x = 0;
        else if (location.x >= hexGrid.gridWidth)
            location.x = hexGrid.gridWidth - 1;

        if (location.y < 0)
            location.y = 0;
        else if (location.y >= hexGrid.gridHeight)
            location.y = hexGrid.gridHeight - 1;

        SetLocation(Mathf.FloorToInt(location.x), Mathf.FloorToInt(location.y));
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
                        foreach (var tile in highlightedTiles)
                        {
                            if (tile.x == x && tile.y == y) 
                            {
                                SetLocation(x, y);
                                RotatePlayer(prevWorldLoc, worldLocation);
                            }
                        }
                        x = hexGrid.hexLocation.GetLength(0);
                        y = hexGrid.hexLocation.GetLength(1);
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

    private void HighlightGrid()
    {
        isGridHighlighted = true;
        string currentLocationType = hexGrid.hexType[location.x, location.y];
        int N = GetMoveDistance(currentLocationType);
        int c = 0;
        int area = GetHexArea(N);
        Vector2Int[] tiles = new Vector2Int[area];
        // get all tiles that need highlighting
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
            Vector2Int[] shapedTiles = new Vector2Int[c];
            for (int i = 0; i < c; i++)
            {
                shapedTiles[i] = tiles[i];
            }
            highlightedTiles = shapedTiles;
        }
        else
        {
            highlightedTiles = tiles;
        }
        // send array of tiles to change their material
        GameObject hex = GameObject.Find("HexGrid");
        foreach (var tile in highlightedTiles)
        {    
            int loc = (tile.y * hexGrid.gridWidth) + tile.x;
            Material tileMat = hex.transform.GetChild(loc).GetChild(0).GetComponent<MeshRenderer>().material;
            cursor.OutlineMaterial(tileMat);
        }
    }

    private void UnhighlightGrid(Vector2Int[] tiles)
    {
        GameObject hex = GameObject.Find("HexGrid");
        foreach (var tile in tiles)
        {
            int loc = (tile.y * hexGrid.gridWidth) + tile.x;
            Material tileMat = hex.transform.GetChild(loc).GetChild(0).GetComponent<MeshRenderer>().material;
            cursor.RemoveOutlineMaterial(tileMat);
        }
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

}
