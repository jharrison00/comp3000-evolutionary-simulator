using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WolfStatsUpdater : MonoBehaviour
{
    public TextMeshProUGUI mutations, starved, speed, strength, vision, energy, puerperal;

    void Update()
    {
        mutations.text = "Mutations: " + WolvesController.Instance.geneticAlgorithm.totalMutations;
        starved.text = "Starved: " + WolvesController.Instance.totalDeaths;

        List<int> averageGenes = WolvesController.Instance.GetAverageGenes();

        speed.text = "Speed: " + averageGenes[0];
        strength.text = "Strength: " + averageGenes[1];
        vision.text = "Vision: " + averageGenes[2];
        energy.text = "Energy: " + averageGenes[3];
        puerperal.text = "Puerperal: " + averageGenes[4];
    }
}
