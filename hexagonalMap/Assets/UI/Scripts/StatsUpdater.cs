using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsUpdater : MonoBehaviour
{
    public TextMeshProUGUI population, chickens, wolves, chicks, pups, totalPopulation, totalChickens, totalWolves, totalBirths, totalDeaths;

    void Update()
    {
        population.text = "Population: " + (ChickensController.Instance.numAnimals + WolvesController.Instance.numAnimals);
        chickens.text = "Chickens: " + ChickensController.Instance.numAnimals;
        wolves.text = "Wolves: " + WolvesController.Instance.numAnimals;
        chicks.text = "Chicks: " + ChickensController.Instance.numBabies;
        pups.text = "Pups: " + WolvesController.Instance.numBabies;
        totalPopulation.text = "Population: " + (ChickensController.Instance.totalAnimals + WolvesController.Instance.totalAnimals);
        totalChickens.text = "Chickens: " + ChickensController.Instance.totalAnimals;
        totalWolves.text = "Wolves: " + WolvesController.Instance.totalAnimals;
        totalBirths.text = "Births: " + (ChickensController.Instance.totalBabiesMade + WolvesController.Instance.totalBabiesMade);
        totalDeaths.text = "Deaths: " + (ChickensController.Instance.totalDeaths + WolvesController.Instance.totalDeaths);
    }
}
