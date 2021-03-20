using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickensController : MonoBehaviour
{
    public static ChickensController Instance;
    public int numChickens;
    public GameObject chickenPrefab;
    public GameObject chickPrefab;
    public Player player;

    public Chicken[] chickens;
    private HexGrid hexGrid;
    public Chicken[] chicks;
    private int numChicks = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        chickens = new Chicken[numChickens];
        hexGrid = HexGrid.Instance;
        SpawnChickens();
    }

    private void SpawnChickens()
    {
        for (int i = 0; i < numChickens; i++)
        {
            GameObject chickenObj = Instantiate(chickenPrefab);
            Chicken chicken = chickenObj.AddComponent<Chicken>();
            chickenObj.name = "Chicken" + i;
            chickens[i] = chicken;
            chickenObj.transform.position = GetRandomSpawnLocation(chicken);
            chickenObj.transform.LookAt(new Vector3(0, chickenObj.transform.position.y, 0));
            chicken.SetBaseStats(3, 3, 3, 6, false);
        }
    }

    private Vector3 GetRandomSpawnLocation(Chicken chicken)
    {
        int x = UnityEngine.Random.Range(0, hexGrid.gridWidth);
        int y = UnityEngine.Random.Range(0, hexGrid.gridHeight);
        chicken.SetLocation(x, y);
        int i = 0;
        while (!IsValidSpawn(chicken.location, chicken)) 
        {
            if (i>50)
                break;
            x = UnityEngine.Random.Range(0, hexGrid.gridWidth);
            y = UnityEngine.Random.Range(0, hexGrid.gridHeight);
            chicken.SetLocation(x, y);
            i++;
        }
        return chicken.worldLocation;
    }

    private bool IsValidSpawn(Vector2Int chickenLocation, Chicken chicken)
    {
        Vector2Int playerLocation = player.location;
        if (playerLocation == chickenLocation)
            return false;
        foreach (Chicken enemyObj in chickens)
        {
            if (chicken != enemyObj && enemyObj != null) 
            {
                if (chicken.location == enemyObj.location)
                    return false;
            }
        }
        string hexType = hexGrid.hexType[chickenLocation.x, chickenLocation.y];
        if (hexType == "Water") 
            return false;
        return true;
    }

    public Vector2Int IsChickenNear(Vector2Int[] tiles, Chicken currentChicken)
    {
        foreach (Vector2Int tile in tiles)
        {
            foreach (var chicken in chickens)
            {
                if (chicken != currentChicken)
                {
                    if (chicken.location == tile)
                    {
                        return tile;
                    }
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    public void GrowUp(Chicken chicken)
    {
        Debug.Log(chicken.name + " has grown up");
        // remove the chick and replace with chicken
        Chicken tempChick = chicken;
        killChick(chicken);

        // redo the chickens array and insert new chicken onto end
        numChickens += 1;
        GameObject newChickenObj = Instantiate(chickenPrefab);
        Chicken newChicken = newChickenObj.AddComponent<Chicken>();
        newChickenObj.name = "Chicken" + (numChickens - 1);
        Chicken[] newChickens = new Chicken[numChickens];
        if (chickens.Length > 0)
        {
            for (int i = 0; i < numChickens - 1; i++)
            {
                newChickens[i] = chickens[i];
            }
        }
        newChickens[numChickens - 1] = newChicken;
        chickens = newChickens;

        // change the chicken object to have the same stats 

        newChickenObj.transform.position = tempChick.worldLocation;
        newChicken.SetLocation(tempChick.location.x, tempChick.location.y);
        newChickenObj.transform.LookAt(new Vector3(0, newChickenObj.transform.position.y, 0));
        newChicken.SetBaseStats(3, 3, 3, 6, false);

    }

    public Chicken GetChickenAtLocation(Vector2Int location)
    {
        foreach (var chicken in chickens)
        {
            if (chicken.location == location)
            {
                return chicken;
            }
        }
        return null;
    }

    public void SendMateSignal(Chicken sender, Chicken recipient)
    {
        recipient.mateCallRecieved = sender;
        sender.mateCallSent = recipient;
    }

    public void Kill(Chicken chicken)
    {
        Debug.Log(chicken.name + " has died");
        int i = 0, x = 0;
        numChickens--;
        Chicken[] newChickens = new Chicken[numChickens];
        foreach (var item in chickens)
        {
            if (item == chicken)
            {
                chickens[i] = null;
            }
            else
            {
                newChickens[x] = item;
                x++;
            }
            i++;
        }
        chickens = newChickens;
        Destroy(chicken.gameObject);
    }

    public void killChick(Chicken chicken)
    {
        int i = 0, x = 0;
        numChicks--;
        Chicken[] newChicks = new Chicken[numChicks];
        foreach (var chick in chicks)
        {
            if (chick == chicken)
            {
                chicks[i] = null;
            }
            else
            {
                newChicks[x] = chick;
                x++;
            }
            i++;
        }
        chicks = newChicks;
        Destroy(chicken.gameObject);
    }

    public void CreateBaby(Chicken sender, Chicken recipient, Vector2Int babyLocation)
    {
        if (recipient.mateCallRecieved != null) 
        {
            Debug.Log(sender.name + " and " + recipient.name + " had a baby");
            sender.mateCallRecieved = null;
            sender.mateCallSent = null;
            sender.hunger += 20;
            sender.movesUntilMating = 6;
            recipient.hunger += 20;
            recipient.movesUntilMating = 6;

            numChicks += 1;
            GameObject chickObj = Instantiate(chickPrefab);
            Chicken chick = chickObj.AddComponent<Chicken>();
            chickObj.name = "Chick" + (numChicks - 1);
            Chicken[] newChicks = new Chicken[numChicks];
            if (chicks.Length > 0) 
            {
                for (int i = 0; i < numChicks - 1; i++) 
                {
                    newChicks[i] = chicks[i];
                }
            }
            newChicks[numChicks - 1] = chick;
            chicks = newChicks;

            Vector3 babyWorldLocation = HexGrid.Instance.CalcWorldPos(babyLocation);
            babyWorldLocation.y = ((HexGrid.Instance.heights[babyLocation.x, babyLocation.y] * 0.1f) * 2f) + 0.2f;
            chickObj.transform.position = babyWorldLocation;
            chickObj.transform.LookAt(new Vector3(0, chickObj.transform.position.y, 0));
            chick.SetLocation(babyLocation.x, babyLocation.y);
            chick.SetBaseStats(1, 1, 1, 3, true);     //EVOLUTION GOES HERE
        }
        else
        {
            sender.mateCallRecieved = null;
            sender.mateCallSent = null;
        }
    }
}
