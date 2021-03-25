using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolvesController : AnimalsController
{
    public static WolvesController Instance;
    public Player player;

    public Wolf[] wolves;
    public Wolf[] pups;

    private int numPups = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        wolves = new Wolf[numAnimals];
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
            wolfObj.name = "Fox" + i;
            wolves[i] = wolf;
            wolfObj.transform.position = GetRandomSpawnLocation(wolf);
            wolfObj.transform.LookAt(new Vector3(0, wolfObj.transform.position.y, 0));
            wolf.SetBaseStats(speed, health, vision, energy, Animal.SpeciesType.Predator, false);   // starter statistics ( TO BE CHANGED BY USER)
        }
    }
}
