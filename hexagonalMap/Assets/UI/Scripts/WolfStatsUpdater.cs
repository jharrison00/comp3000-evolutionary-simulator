using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WolfStatsUpdater : MonoBehaviour
{
    public TextMeshProUGUI mutations, starved;

    void Update()
    {
        mutations.text = "Mutations: " + WolvesController.Instance.geneticAlgorithm.totalMutations;
        starved.text = "Starved: " + WolvesController.Instance.totalDeaths;
    }
}
