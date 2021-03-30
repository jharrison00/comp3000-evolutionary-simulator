using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChickenStatsUpdater : MonoBehaviour
{
    public TextMeshProUGUI mutations, starved, eaten;

    void Update()
    {
        mutations.text = "Mutations: " + ChickensController.Instance.geneticAlgorithm.totalMutations;
        starved.text = "Starved: " + ChickensController.Instance.starved;
        eaten.text = "Eaten: " + ChickensController.Instance.eaten;
    }
}
