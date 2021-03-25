using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalsController : MonoBehaviour
{
    public int numAnimals;
    public int totalAnimals;
    public GameObject animalPrefab;
    public GameObject babyAnimalPrefab;
    public GeneticAlgorithm geneticAlgorithm;
    public int speed, health, vision, energy, hunger;
    public HexGrid hexGrid;

    public Vector3 GetRandomSpawnLocation(Animal animal)
    {
        int x = Random.Range(0, hexGrid.gridWidth);
        int y = Random.Range(0, hexGrid.gridHeight);
        animal.SetLocation(x, y);
        int i = 0;
        while (!IsValidSpawn(animal.location, animal))
        {
            if (i > 50)
                break;
            x = Random.Range(0, hexGrid.gridWidth);
            y = Random.Range(0, hexGrid.gridHeight);
            animal.SetLocation(x, y);
            i++;
        }
        return animal.worldLocation;
    }

    public bool IsValidSpawn(Vector2Int animalLocation, Animal animal)
    {
        foreach (Animal animalObj in ChickensController.Instance.chickens)
        {
            if (animal != animalObj && animalObj != null)
            {
                if (animal.location == animalObj.location)
                    return false;
            }
        }
        foreach (Animal animalObj in WolvesController.Instance.wolves)
        {
            if (animal != animalObj && animalObj != null)
            {
                if (animal.location == animalObj.location)
                    return false;
            }
        }
        string hexType = hexGrid.hexType[animalLocation.x, animalLocation.y];
        if (hexType == "Water")
            return false;
        return true;
    }
}
