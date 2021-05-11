using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChickenStatsUpdater : MonoBehaviour
{
    public TextMeshProUGUI mutations, starved, eaten, speed, strength, vision, energy, puerperal;

    void Update()
    {
        mutations.text = "Mutations: " + ChickensController.Instance.geneticAlgorithm.totalMutations;
        starved.text = "Starved: " + ChickensController.Instance.starved;
        eaten.text = "Eaten: " + ChickensController.Instance.eaten;

        List<int> averageGenes = ChickensController.Instance.GetAverageGenes();

        speed.text = "Speed: " + averageGenes[0];
        strength.text = "Strength: " + averageGenes[1];
        vision.text = "Vision: " + averageGenes[2];
        energy.text = "Energy: " + averageGenes[3];
        puerperal.text = "Puerperal: " + averageGenes[4];
    }
}
