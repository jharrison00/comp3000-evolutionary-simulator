using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DataLog : MonoBehaviour
{
    public List<int> totalPopulation = new List<int>();
    public List<int> wolfPopulation = new List<int>();
    public List<int> chickenPopulation = new List<int>();
    public List<int> wolfSpeed = new List<int>();
    public List<int> wolfStrength = new List<int>();
    public List<int> wolfVision = new List<int>();
    public List<int> wolfEnergy = new List<int>();
    public List<int> wolfPuerperal = new List<int>();
    public List<int> chickenSpeed = new List<int>();
    public List<int> chickenStrength = new List<int>();
    public List<int> chickenVision = new List<int>();
    public List<int> chickenEnergy = new List<int>();
    public List<int> chickenPuerperal = new List<int>();

    private float timer;
    private float generationTime = 20f;
    public bool isNewData = false;

    public void Start()
    {
        UpdateData();
    }

    public void Update()
    {
        timer += Time.deltaTime;

        if (timer >= generationTime)
        {
            UpdateData();         
            timer = 0f;
            isNewData = true;
        }
        else
        {
            isNewData = false;
        }
    }

    private void UpdateData()
    {
        UpdatePopulation();
        UpdateWolves();
        UpdateChickens();
    }

    private void UpdatePopulation()
    {
        int updatedPopulation = ChickensController.Instance.totalAnimals + WolvesController.Instance.totalAnimals + ChickensController.Instance.numBabies + WolvesController.Instance.numBabies;
        totalPopulation.Add(updatedPopulation);
        int updatedChickenPopulation = ChickensController.Instance.totalAnimals + ChickensController.Instance.numBabies;
        chickenPopulation.Add(updatedChickenPopulation);
        int updatedWolfPopulation = WolvesController.Instance.totalAnimals + WolvesController.Instance.numBabies;
        wolfPopulation.Add(updatedWolfPopulation);
    }

    private void UpdateWolves()
    {
        List<int> genes = WolvesController.Instance.GetAverageGenes();
        wolfSpeed.Add(genes[0]);
        wolfStrength.Add(genes[1]);
        wolfVision.Add(genes[2]);
        wolfEnergy.Add(genes[3]);
        wolfPuerperal.Add(genes[4]);
    }

    private void UpdateChickens()
    {
        List<int> genes = ChickensController.Instance.GetAverageGenes();
        chickenSpeed.Add(genes[0]);
        chickenStrength.Add(genes[1]);
        chickenVision.Add(genes[2]);
        chickenEnergy.Add(genes[3]);
        chickenPuerperal.Add(genes[4]);
    }

}
