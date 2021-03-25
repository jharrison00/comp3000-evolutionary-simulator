using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickensController : AnimalsController
{
    public static ChickensController Instance;
    public Player player;

    public Chicken[] chickens;
    public Chicken[] chicks;

    private int numChicks = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        chickens = new Chicken[numAnimals];
        totalAnimals = numAnimals;
        hexGrid = HexGrid.Instance;
        geneticAlgorithm = GetComponent<GeneticAlgorithm>();
        SpawnChickens();
    }

    private void SpawnChickens()
    {
        for (int i = 0; i < numAnimals; i++)
        {
            GameObject chickenObj = Instantiate(animalPrefab);
            Chicken chicken = chickenObj.AddComponent<Chicken>();
            chickenObj.name = "Chicken" + i;
            chickens[i] = chicken;
            chickenObj.transform.position = GetRandomSpawnLocation(chicken);
            chickenObj.transform.LookAt(new Vector3(0, chickenObj.transform.position.y, 0));
            chicken.SetBaseStats(speed, health, vision, energy, Animal.SpeciesType.Prey, false);    // starter statistics ( TO BE CHANGED BY USER)
        }
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
        totalAnimals++;
        // remove the chick and replace with chicken
        Chicken tempChick = chicken;
        killChick(chicken);

        // redo the chickens array and insert new chicken onto end
        numAnimals += 1;
        GameObject newChickenObj = Instantiate(animalPrefab);
        Chicken newChicken = newChickenObj.AddComponent<Chicken>();
        newChickenObj.name = "Chicken" + totalAnimals;
        Chicken[] newChickens = new Chicken[numAnimals];
        if (chickens.Length > 0)
        {
            for (int i = 0; i < numAnimals - 1; i++)
            {
                newChickens[i] = chickens[i];
            }
        }
        newChickens[numAnimals - 1] = newChicken;
        chickens = newChickens;

        // change the chicken object to have the same stats 

        newChickenObj.transform.position = tempChick.worldLocation;
        newChicken.SetLocation(tempChick.location.x, tempChick.location.y);
        newChickenObj.transform.LookAt(new Vector3(0, newChickenObj.transform.position.y, 0));
        newChicken.SetBaseStats(tempChick.speed, tempChick.health, tempChick.vision, tempChick.energy, Animal.SpeciesType.Prey, false);  
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
        numAnimals--;
        Chicken[] newChickens = new Chicken[numAnimals];
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
            GameObject chickObj = Instantiate(babyAnimalPrefab);
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
            // use genetic algorithm to decide offsprings statistics

            int[] stats = geneticAlgorithm.Begin(chick, sender, recipient);
            chick.SetBaseStats(stats[0], stats[1], stats[2], stats[3], Animal.SpeciesType.Prey, true);
        }
        else
        {
            sender.mateCallRecieved = null;
            sender.mateCallSent = null;
        }
    }
}
