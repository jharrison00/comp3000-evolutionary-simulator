using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalsController : MonoBehaviour
{
    public int numAnimals;
    public int totalAnimals;
    public int numBabies = 0;
    public int totalBabiesMade;
    public int totalDeaths = 0;
    public Animal[] animals;
    public Animal[] babyAnimals;
    public GameObject animalPrefab;
    public GameObject babyAnimalPrefab;
    public GeneticAlgorithm geneticAlgorithm;
    public int speed, strength, vision, energy, puerperal;
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
        foreach (Animal animalObj in ChickensController.Instance.animals)
        {
            if (animal != animalObj && animalObj != null)
            {
                if (animal.location == animalObj.location)
                    return false;
            }
        }
        foreach (Animal animalObj in WolvesController.Instance.animals)
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

    public Chicken GetChickenAtLocation(Vector2Int location)
    {
        foreach (var chicken in ChickensController.Instance.animals) 
        {
            if (chicken.location == location)
            {
                return (Chicken)chicken;
            }
        }
        return null;
    }

    public Wolf GetWolfAtLocation(Vector2Int location)
    {
        foreach (var wolf in WolvesController.Instance.animals)
        {
            if (wolf.location == location)
            {
                return (Wolf)wolf;
            }
        }
        return null;
    }

    public void SendMateSignal(Animal sender, Animal recipient)
    {
        recipient.mateCall = sender;
    }

    public void CreateBaby(Animal sender, Animal recipient, Vector2Int babyLocation)
    {
        totalBabiesMade++;
        bool chicken = false;

        numBabies += 1;
        GameObject babyObj = Instantiate(babyAnimalPrefab);
        Animal baby;
        if (sender.GetType().Name == "Chicken")
        {
            chicken = true;
            baby = babyObj.AddComponent<Chicken>();
            babyObj.name = "Chick" + (numBabies - 1);
        }
        else
        {
            baby = babyObj.AddComponent<Wolf>();
            babyObj.name = "Pup" + (numBabies - 1);
        }

        sender.movesUntilMating = puerperal;
        recipient.movesUntilMating = puerperal;

        Animal[] newBabies = new Animal[numBabies];
        if (babyAnimals.Length > 0)
        {
            for (int i = 0; i < numBabies - 1; i++)
            {
                newBabies[i] = babyAnimals[i];
            }
        }
        newBabies[numBabies - 1] = baby;
        babyAnimals = newBabies;

        Vector3 babyWorldLocation = HexGrid.Instance.CalcWorldPos(babyLocation);
        babyWorldLocation.y = ((HexGrid.Instance.heights[babyLocation.x, babyLocation.y] * 0.1f) * 2f) + 0.2f;
        babyObj.transform.position = babyWorldLocation;
        babyObj.transform.LookAt(new Vector3(0, babyObj.transform.position.y, 0));
        baby.SetLocation(babyLocation.x, babyLocation.y);
        // use genetic algorithm to decide offsprings statistics
        int[] stats = geneticAlgorithm.Begin(sender, recipient);
        baby.transform.parent = this.transform;
        if (chicken)
            baby.SetBaseStats(stats[0], stats[1], stats[2], stats[3], stats[4], Animal.SpeciesType.Prey, true);
        else
            baby.SetBaseStats(stats[0], stats[1], stats[2], stats[3], stats[4], Animal.SpeciesType.Predator, true);
    }

    public void Kill(Animal animal)
    {
        int i = 0, x = 0;
        numAnimals--;
        Animal[] newAnimals = new Animal[numAnimals];
        foreach (var item in animals)
        {
            if (item == animal)
            {
                animals[i] = null;
            }
            else
            {
                newAnimals[x] = item;
                x++;
            }
            i++;
        }
        animals = newAnimals;
        Destroy(animal.gameObject);
    }

    public void KillBaby(Animal animal)
    {
        int i = 0, x = 0;
        numBabies--;
        Animal[] newBabies = new Animal[numBabies];
        foreach (var baby in babyAnimals)
        {
            if (baby == animal)
            {
                babyAnimals[i] = null;
            }
            else
            {
                newBabies[x] = baby;
                x++;
            }
            i++;
        }
        babyAnimals = newBabies;
        Destroy(animal.gameObject);
    }

    public void GrowUp(Animal animal)
    {
        totalAnimals++;
        bool chicken = false;
        // remove the baby and replace with adult
        Animal tempAnimal = animal;
        KillBaby(animal);

        // redo the animals array and insert new animal onto end
        numAnimals += 1;
        GameObject newAnimalObj = Instantiate(animalPrefab);
        Animal newAnimal;
        if (animal.GetType().Name == "Chicken")
        {
            chicken = true;
            newAnimal = newAnimalObj.AddComponent<Chicken>();
            newAnimalObj.name = "Chicken" + totalAnimals;
        }
        else
        {
            newAnimal = newAnimalObj.AddComponent<Wolf>();
            newAnimalObj.name = "Wolf" + totalAnimals;
        }
        Animal[] newAnimals = new Animal[numAnimals];
        if (animals.Length > 0)
        {
            for (int i = 0; i < numAnimals - 1; i++)
            {
                newAnimals[i] = animals[i];
            }
        }
        newAnimals[numAnimals - 1] = newAnimal;
        animals = newAnimals;
        // change the animal object to have the same stats 
        newAnimalObj.transform.position = tempAnimal.worldLocation;
        newAnimal.SetLocation(tempAnimal.location.x, tempAnimal.location.y);
        newAnimalObj.transform.LookAt(new Vector3(0, newAnimalObj.transform.position.y, 0));
        newAnimal.transform.parent = this.transform;
        if (chicken)
            newAnimal.SetBaseStats(tempAnimal.speed, tempAnimal.strength, tempAnimal.vision, tempAnimal.energy, tempAnimal.puerperal, Animal.SpeciesType.Prey, false);
        else
            newAnimal.SetBaseStats(tempAnimal.speed, tempAnimal.strength, tempAnimal.vision, tempAnimal.energy, tempAnimal.puerperal, Animal.SpeciesType.Predator, false);
    }

    public List<int> GetAverageGenes()
    {
        int speed = 0, strength = 0, vision = 0, energy = 0, puerperal = 0;
        foreach (Animal animal in animals)
        {
            speed += animal.speed;
            strength += animal.strength;
            vision += animal.vision;
            energy += animal.energy;
            puerperal += animal.puerperal;
        }
        speed = speed / animals.Length;
        strength = strength / animals.Length;
        vision = vision / animals.Length;
        energy = energy / animals.Length;
        puerperal = puerperal / animals.Length;
        return new List<int> { speed, strength, vision, energy, puerperal };
    }
}
