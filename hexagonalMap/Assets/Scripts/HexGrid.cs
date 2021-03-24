using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public static HexGrid Instance;

    public Transform[] hexagonPrefabs = new Transform[3];
    public Transform[] treePrefabs = new Transform[3];
    public Transform[] rockPrefabs = new Transform[24];
    public Transform foodPrefab;
    public int gridWidth;
    public int gridHeight;
    public int gridDepth;
    public int noiseScale;
    public AnimationCurve noiseCurve;
    public float gap = 0.0f;
    [Range(1,100)]
    public int foodPercentage;
    public int foodTiles;

    private Texture2D perlinTexture;
    public float[,] heights;
    public string[,] hexType;
    public string[,] hexLocation;

    float hexWidth = 1.732f;
    float hexHeight = 2f;

    Vector3 startPos;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreatePerlinTexture();
        AddGap();
        CalcStartPos();
        CreateGrid();
        SpawnFood();
        foodTiles = GetFoodTiles();
        Instance = this;
    }


    void CreatePerlinTexture()
    {
        PerlinGenerator perlin = new PerlinGenerator();
        perlinTexture = perlin.GenerateTexture();
    }

    void AddGap()
    {
        hexWidth = 1.732f;
        hexHeight = 2f;
        hexWidth += hexWidth * gap;
        hexHeight += hexHeight * gap;
    }

    void CalcStartPos()
    {
        float offset = 0;
        if (gridHeight / 2 % 2 != 0)
            offset = hexWidth / 2;
        float x = -hexWidth * (gridWidth / 2) - offset;
        float z = hexHeight * 0.75f * (gridHeight / 2);
        startPos = new Vector3(x, 0, z);
    }

    public Vector3 CalcWorldPos(Vector2 gridPos)
    {
        float offset = 0;
        if (gridPos.y % 2 != 0)
            offset = hexWidth / 2;
        float x = startPos.x + gridPos.x * hexWidth + offset;
        float z = startPos.z - gridPos.y * hexHeight * 0.75f;
        return new Vector3(x, 0.0f, z);
    }

    float addNoise(Vector2 gridPos)
    {
        float offset = 0;
        if (gridPos.y % 2 != 0)
            offset = hexWidth / 2;
        float xNoise = startPos.x + gridPos.x * hexWidth + offset;
        float zNoise = startPos.z - gridPos.y * hexHeight * 0.75f;
        float yNoise = perlinTexture.GetPixel(Mathf.FloorToInt(xNoise), Mathf.FloorToInt(zNoise)).grayscale;
        return yNoise;
    }

    void CreateGrid()
    {
        heights = new float[gridWidth, gridHeight];
        hexType = new string[gridWidth, gridHeight];
        hexLocation = new string[gridWidth, gridHeight];
        float minHeight = Mathf.Infinity, maxHeight = 0f;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2 gridPos = new Vector2(x, y);
                float yNoise = addNoise(gridPos);
                float scaleChange = gridDepth + (yNoise * noiseScale * noiseCurve.Evaluate(yNoise * noiseScale));
                heights[x, y] = scaleChange;
                if (scaleChange <= minHeight)
                    minHeight = scaleChange;
                if (scaleChange >= maxHeight)
                    maxHeight = scaleChange;
            }
        }

        float range = maxHeight - minHeight;
        float ranges = range / hexagonPrefabs.Length;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2 gridPos = new Vector2(x, y);
                Transform hex;
                if (heights[x, y] >= minHeight && heights[x, y] < minHeight + ranges)
                {
                    hex = Instantiate(hexagonPrefabs[0]) as Transform;
                    hex.name = "Water" + x + "," + y;
                    hexType[x, y] = "Water";
                }
                else if (heights[x, y] >= minHeight + ranges && heights[x, y] <= maxHeight - ranges + 1)
                {
                    hex = Instantiate(hexagonPrefabs[1]) as Transform;
                    int rand = UnityEngine.Random.Range(0, (gridWidth * gridHeight));
                    if (rand >= 0 && rand < (gridWidth * gridHeight) / 9)
                    {
                        AddElements(gridPos, heights[x, y], treePrefabs, hex);
                        hex.name = "Trees" + x + "," + y;
                        hexType[x, y] = "Trees";
                    }
                    else
                    {
                        hex.name = "Grass" + x + "," + y;
                        hexType[x, y] = "Grass";
                    }
                }
                else
                {
                    hex = Instantiate(hexagonPrefabs[2]) as Transform;
                    int rand = UnityEngine.Random.Range(0, (gridWidth * gridHeight));
                    if (rand >= 0 && rand <= (gridWidth * gridHeight) / 6)
                    {
                        AddElements(gridPos, heights[x, y], rockPrefabs, hex);
                        hex.name = "Rocks" + x + "," + y;
                        hexType[x, y] = "Rocks";
                    }
                    else
                    {
                        hex.name = "Stone" + x + "," + y;
                        hexType[x, y] = "Stone";
                    }
                }

                hexLocation[x, y] = hex.name;
                hex.position = CalcWorldPos(gridPos);
                Vector3 scale = new Vector3(0.0f, heights[x, y], 0.0f);
                hex.localScale += scale;
                hex.parent = this.transform;
                hex.gameObject.AddComponent<BoxCollider>();
                BoxCollider boxCollider = hex.gameObject.GetComponent<BoxCollider>();
                boxCollider.size = new Vector3(hexWidth, 0.2f, hexHeight);
                boxCollider.center = new Vector3(0, 0.1f, 0);
            }
        }
    }

    private void SpawnFood()
    {
        //Get all available grass tiles
        int numGrassTiles = 0;
        foreach (var tile in hexType)
        {
            if (tile == "Grass")
            {
                numGrassTiles++;
            }
        }
        // Store grass tiles in array
        Vector2Int[] grassTiles = new Vector2Int[numGrassTiles];
        int counter = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (hexType[x,y] == "Grass")
                {
                    grassTiles[counter] = new Vector2Int(x, y);
                    counter++;
                }
            }
        }
        // Get amount of tiles to be converted to food ( based on percentage - foodPopulation)
        float foodTilesF = numGrassTiles * (foodPercentage / 100f);
        int foodTiles = Mathf.FloorToInt(foodTilesF);
        // Convert random grass tiles to food
        for (int i = foodTiles; i > 0; i--)
        {
            int rand = UnityEngine.Random.Range(0, grassTiles.Length);
            Vector2Int tile = grassTiles[rand];
            Transform hex = transform.GetChild((tile.y * gridWidth) + tile.x);
            AddFood(tile, heights[tile.x, tile.y], foodPrefab, hex);
            hex.name = "Vegetation" + tile.x + "," + tile.y;
            hexType[tile.x, tile.y] = "Vegetation";
            hexLocation[tile.x, tile.y] = hex.name;
            var obj = transform.GetChild((tile.y * gridWidth) + tile.x).GetChild(2);
            obj.localScale += new Vector3(0, obj.localScale.y * heights[tile.x, tile.y], 0f);
            // replace grassTiles array to not include this tile
            numGrassTiles--;
            Vector2Int[] newGrassTiles = new Vector2Int[numGrassTiles];
            bool removed = false;
            for (int c = 0; c < numGrassTiles; c++)
            {
                if (grassTiles[c] == new Vector2Int(tile.x, tile.y))
                {
                    newGrassTiles[c] = grassTiles[c + 1];
                    c++;
                    removed = true;
                }
                else
                {
                    if (removed)
                        newGrassTiles[c - 1] = grassTiles[c];
                    else
                        newGrassTiles[c] = grassTiles[c];
                }
            }
            grassTiles = newGrassTiles;
        }
    }

    private int GetFoodTiles()
    {
        int foodTiles = 0;
        foreach (var tile in hexType)
        {
            if (tile == "Vegetation")
            {
                foodTiles++;
            }
        }
        return foodTiles;
    }
    private void AddFood(Vector2 gridPos, float scale, Transform foodPrefab, Transform hex)
    {
        Vector3 elementLocation = new Vector3();
        elementLocation.x = 0;
        elementLocation.z = 0;
        elementLocation.y = 0.2f;

        Transform food;
        food = Instantiate(foodPrefab);
        food.position = elementLocation;
        food.localScale -= new Vector3(0, food.localScale.y - food.localScale.y / scale, 0f);
        food.parent = hex;
        food.localPosition = elementLocation;
    }

    private void AddElements(Vector2 gridPos, float scale, Transform[] prefabType, Transform hex)
    {
        float xRange = (hexWidth - 1f) / 2f;
        float zRange = (hexHeight - 1f) / 2f;
        int numElements = UnityEngine.Random.Range(1, 4);
        Vector3[] elements = new Vector3[numElements];

        for (int i = 0; i < numElements; i++)
        {
            Vector3 elementLocation = new Vector3();

            int typeOfElement = UnityEngine.Random.Range(0, prefabType.Length);

            float randX = UnityEngine.Random.Range(-xRange, xRange);
            float randZ = UnityEngine.Random.Range(-zRange, zRange);

            elementLocation.x += randX;
            elementLocation.z += randZ;
            elementLocation.y = 0.2f;

            if (i > 0)
                elementLocation = DoElementsCollide(elements, elementLocation);

            int rotateY = UnityEngine.Random.Range(0, 360);

            Transform element;
            element = Instantiate(prefabType[typeOfElement]);
            element.position = elementLocation;
            element.eulerAngles = new Vector3(0, rotateY, 0);
            element.localScale -= new Vector3(0, element.localScale.y - element.localScale.y / scale, 0f);
            element.parent = hex;
            element.localPosition = elementLocation;

            elements[i] = element.position;
        }   
    }

    private Vector3 DoElementsCollide(Vector3[] elements,  Vector3 element)
    {
        // if the tree is within a 0.2 radius of another tree
        for (int i = 0; i < elements.Length; i++)
        {
            float xProximity = element.x - elements[i].x;
            float zProximity = element.z - elements[i].z;

            if (xProximity <= 0.2 && xProximity >= 0) 
            {
                element.x += 0.2f;
            }
            if (xProximity >= -0.2 && xProximity <= 0) 
            {
                if (element.x < 0)
                    element.x += 0.2f;
                else
                    element.x -= 0.2f;
            }
            if (zProximity <= 0.2f && zProximity >= 0)  
            {
                element.z += 0.2f;
            }
            if (zProximity >= -0.2 && zProximity <= 0)
            {
                if (element.z < 0)
                    element.z += 0.2f;
                else
                    element.z -= 0.2f;
            }
        }
        return element;
    }

    public Vector2Int CubeToOddR(int x, int y, int z)
    {
        int col = x + (z - (z & 1)) / 2;
        int row = z;
        Vector2Int colrow = new Vector2Int(col, row);
        return colrow;
    }
    public Vector3Int OddRToCube(int col, int row)
    {
        int x = col - (row - (row & 1)) / 2;
        int z = row;
        int y = -x - z;
        Vector3Int xyz = new Vector3Int(x, y, z);
        return xyz;
    }
    public int CubeDistance(Vector3Int currLoc, Vector3Int moveLoc)
    {
        // distance from currLoc to moveLoc
        int x, y, z;
        x = moveLoc.x - currLoc.x;
        y = moveLoc.y - currLoc.y;
        z = moveLoc.z - currLoc.z;
        return (Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z)) / 2;
    }

    public void RespawnFood(Vector2Int gridPos)
    {
        // Delete food at location and rename tile to grass
        int loc = (gridPos.y * gridWidth) + gridPos.x;
        if (transform.GetChild(loc).childCount > 2)
        {
            var obj = transform.GetChild(loc).GetChild(2);
            Destroy(obj.gameObject);
            hexType[gridPos.x, gridPos.y] = "Grass";
            transform.GetChild(loc).name = "Grass" + gridPos.x + "," + gridPos.y;
        }
        // Create food at different area
        if (GetFoodTiles() == foodTiles - 1) 
        {
            while (true)
            {
                int x = UnityEngine.Random.Range(0, gridWidth);
                int y = UnityEngine.Random.Range(0, gridHeight);
                if (hexType[x, y] == "Grass")
                {
                    Transform hex = transform.GetChild((y * gridWidth) + x);
                    AddFood(new Vector2(x, y), heights[x, y], foodPrefab, hex);
                    var obj = transform.GetChild((y * gridWidth) + x).GetChild(2);
                    obj.localScale += new Vector3(0, obj.localScale.y * heights[x, y], 0f);
                    hex.name = "Vegetation" + x + "," + y;
                    hexType[x, y] = "Vegetation";
                    hexLocation[x, y] = hex.name;
                    break;
                }
            }
        }
    }
}