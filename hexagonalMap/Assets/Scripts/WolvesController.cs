using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolvesController : AnimalsController
{
    public static WolvesController Instance;
    public Player player;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        animals = new Wolf[numAnimals];
        totalAnimals = numAnimals;
        hexGrid = HexGrid.Instance;
        geneticAlgorithm = GetComponent<GeneticAlgorithm>();
        SpawnWolves();
    }

    private void SpawnWolves()
    {
        for (int i = 0; i < numAnimals; i++)
        {
            GameObject wolfObj = Instantiate(animalPrefab);
            Wolf wolf = wolfObj.AddComponent<Wolf>();
            wolfObj.name = "Wolf" + i;
            animals[i] = wolf;
            wolfObj.transform.position = GetRandomSpawnLocation(wolf);
            wolfObj.transform.LookAt(new Vector3(0, wolfObj.transform.position.y, 0));
            wolf.SetBaseStats(speed, strength, vision, energy, puerperal, Animal.SpeciesType.Predator, false);   // starter statistics ( TO BE CHANGED BY USER)
            wolf.transform.parent = this.transform;
        }
    }

    public Vector2Int IsWolfNear(Vector2Int[] tiles, Wolf currentWolf)
    {
        foreach (Vector2Int tile in tiles)
        {
            foreach (var wolf in animals)
            {
                if (wolf != currentWolf)
                {
                    if (wolf.location == tile)
                    {
                        return tile;
                    }
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    public void EatChicken(Chicken chicken, Wolf wolf)
    {
        Debug.Log(wolf.name + " has eaten " + chicken.name);
        if (chicken.isBaby)
            ChickensController.Instance.KillBaby(chicken);
        else
            ChickensController.Instance.Kill(chicken);
        ChickensController.Instance.totalDeaths++;
        ChickensController.Instance.eaten++;
    }
}
