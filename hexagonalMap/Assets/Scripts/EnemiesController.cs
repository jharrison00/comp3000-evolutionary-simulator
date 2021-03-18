using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesController : MonoBehaviour
{
    public static EnemiesController Instance;
    public int numEnemies;
    public GameObject enemyPrefab;
    public Player player;

    public Enemy[] enemies;
    private HexGrid hexGrid;

    private void Awake()
    {
        Instance = this;
    }


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
            enemyObj.SetBaseStats();
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

    public void MoveEnemies()
    { 
        foreach (Enemy enemy in enemies)
	    {
            enemy.ChooseMove();
	    }
    }

    public Vector2Int IsPlayerNear(Vector2Int[] tiles, Enemy currentEnemy)
    {
        foreach (Vector2Int tile in tiles)
        {
            // Check if player is on tile or if enemies are on tile
            if (player.location == tile)
            {
                return tile;
            }
            foreach (var enemy in enemies)
            {
                if (enemy!=currentEnemy)
                {
                    if (enemy.location == tile)
                    {
                        return tile;
                    }
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    public void Kill(Enemy enemy)
    {
        Debug.Log(enemy.name + " has died");
        int i = 0, x = 0;
        numEnemies--;
        Enemy[] newEnemies = new Enemy[numEnemies];
        foreach (var item in enemies)
        {
            if (item == enemy)
            {
                enemies[i] = null;
            }
            else
            {
                newEnemies[x] = item;
                x++;
            }
            i++;
        }
        enemies = newEnemies;
        Destroy(enemy.gameObject);
    }
}
