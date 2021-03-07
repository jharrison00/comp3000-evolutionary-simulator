using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesController : MonoBehaviour
{
    public int numEnemies;
    public GameObject enemyPrefab;
    public Player player;

    private Enemy[] enemies;
    private HexGrid hexGrid;

    void Start()
    {
        enemies = new Enemy[numEnemies];
        hexGrid = HexGrid.Instance;
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < numEnemies; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            Enemy enemyObj = enemy.AddComponent<Enemy>();
            enemy.name = "Enemy" + i;
            enemies[i] = enemyObj;
            enemy.transform.position = GetRandomSpawnLocation(enemyObj);
            enemy.transform.LookAt(new Vector3(0, enemy.transform.position.y, 0));
            enemyObj.DoFunction();
        }
    }

    private Vector3 GetRandomSpawnLocation(Enemy enemy)
    {
        int x = UnityEngine.Random.Range(0, hexGrid.gridWidth);
        int y = UnityEngine.Random.Range(0, hexGrid.gridHeight);
        enemy.SetLocation(x, y);
        int i = 0;
        while (!IsValidSpawn(enemy.location, enemy)) 
        {
            if (i>50)
                break;
            x = UnityEngine.Random.Range(0, hexGrid.gridWidth);
            y = UnityEngine.Random.Range(0, hexGrid.gridHeight);
            enemy.SetLocation(x, y);
            i++;
        }
        return enemy.worldLocation;
    }

    private bool IsValidSpawn(Vector2Int enemyLocation, Enemy enemy)
    {
        Vector2Int playerLocation = player.location;
        if (playerLocation == enemyLocation)
            return false;
        foreach (Enemy enemyObj in enemies)
        {
            if (enemy != enemyObj && enemyObj != null) 
            {
                if (enemy.location == enemyObj.location)
                    return false;
            }
        }
        string hexType = hexGrid.hexType[enemyLocation.x, enemyLocation.y];
        if (hexType == "Water") 
            return false;
        return true;
    }
}
