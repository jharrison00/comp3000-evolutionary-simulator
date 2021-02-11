using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public Transform[] hexagonPrefabs = new Transform[3];
    public Transform[] treePrefabs = new Transform[3];
    public int gridWidth;
    public int gridHeight;
    public int gridDepth;
    public int noiseScale;
    public AnimationCurve noiseCurve;

    private Texture2D perlinTexture;

    float hexWidth = 1.732f;
    float hexHeight = 2f;
    public float gap = 0.0f;

    Vector3 startPos;
    void Start()
    {
        CreatePerlinTexture();
        AddGap();
        CalcStartPos();
        CreateGrid();
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

    Vector3 CalcWorldPos(Vector2 gridPos)
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
        float[,] heights = new float[gridWidth, gridHeight];
        float minHeight = 9999f, maxHeight = 0f;
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
                }
                else if (heights[x, y] >= minHeight + ranges && heights[x, y] <= maxHeight - ranges)
                {
                    hex = Instantiate(hexagonPrefabs[1]) as Transform;
                    int rand = UnityEngine.Random.Range(0, (gridWidth * gridHeight));
                    if (rand >= 0 && rand <= (gridWidth * gridHeight) / 9) 
                    {
                        AddTrees(gridPos, heights[x, y]);
                        hex.name = "Trees" + x + "," + y;
                    }
                    else
                        hex.name = "Grass" + x + "," + y;
                }
                else
                {
                    hex = Instantiate(hexagonPrefabs[2]) as Transform;
                    hex.name = "Stone" + x + "," + y;
                }

                hex.position = CalcWorldPos(gridPos);
                Vector3 scale = new Vector3(0.0f, heights[x, y], 0.0f);
                hex.localScale += scale;
                hex.parent = this.transform;
            }
        }
    }

    private void AddTrees(Vector2 gridPos, float scale)
    {
        float xRange = (hexWidth - 1f) / 2f;
        float zRange = (hexHeight - 1f) / 2f;
        int numTrees = UnityEngine.Random.Range(1, 4);
        Vector3[] trees = new Vector3[numTrees];

        for (int i = 0; i < numTrees; i++)
        {
            Vector3 treeLocation = CalcWorldPos(gridPos);

            int typeOfTree = UnityEngine.Random.Range(0, treePrefabs.Length);

            float randX = UnityEngine.Random.Range(-xRange, xRange);
            float randZ = UnityEngine.Random.Range(-zRange, zRange);

            treeLocation.x += randX;
            treeLocation.z += randZ;
            treeLocation.y = ((scale * 0.1f) * 2f) + 0.2f;

            if (i > 0)
                treeLocation = DoTreesCollide(trees, treeLocation);

            int rotateY = UnityEngine.Random.Range(0, 360);

            Transform tree;
            tree = Instantiate(treePrefabs[typeOfTree]);
            tree.position = treeLocation;
            tree.eulerAngles = new Vector3(0, rotateY, 0);
            tree.parent = this.transform;
            trees[i] = tree.position;
        }   
    }

    private Vector3 DoTreesCollide(Vector3[] trees,  Vector3 tree)
    {
        // if the tree is within a 0.2 radius of another tree
        for (int i = 0; i < trees.Length; i++)
        {
            float xProximity = tree.x - trees[i].x;
            float zProximity = tree.z - trees[i].z;

            if (xProximity <= 0.2 && xProximity >= 0) 
            {
                tree.x += 0.2f;
            }
            if (xProximity >= -0.2 && xProximity <= 0) 
            {
                if (tree.x < 0)
                    tree.x += 0.2f;
                else
                    tree.x -= 0.2f;
            }
            if (zProximity <= 0.2f && zProximity >= 0)  
            {
                tree.z += 0.2f;
            }
            if (zProximity >= -0.2 && zProximity <= 0)
            {
                if (tree.z < 0) 
                    tree.z += 0.2f;
                else
                    tree.z -= 0.2f;
            }
        }
        return tree;
    }
}