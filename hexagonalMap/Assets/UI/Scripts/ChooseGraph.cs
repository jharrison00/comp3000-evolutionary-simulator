using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ChooseGraph : MonoBehaviour
{
    public GameObject dataObj;
    public GameObject graphObj;
    public TextMeshProUGUI titleText;

    private GraphWindow graph;
    private DataLog data;

    private List<int> currentList;
    private string currentDataType;

    public void Start()
    {
        graph = graphObj.GetComponent<GraphWindow>();
        data = dataObj.GetComponent<DataLog>();
    }

    public void Update()
    {
        if (data.isNewData)
        {
            SetList(currentDataType);
        }
    }

    public void SetList(string dataType)
    {
        currentDataType = dataType;
        string title = "";
        switch (dataType)
        {
            case "TotalPop":
                currentList = data.totalPopulation;
                title = "Total Population";
                break;
            case "WolfPop":
                currentList = data.wolfPopulation;
                title = "Wolf Population";
                break;
            case "ChickenPop":
                currentList = data.chickenPopulation;
                title = "Chicken Population";
                break;
            case "WolfSpeed":
                currentList = data.wolfSpeed;
                title = "Wolf Speed";
                break;
            case "WolfStrength":
                currentList = data.wolfStrength;
                title = "Wolf Strength";
                break;
            case "WolfVision":
                currentList = data.wolfVision;
                title = "Wolf Vision";
                break;
            case "WolfEnergy":
                currentList = data.wolfEnergy;
                title = "Wolf Energy";
                break;
            case "WolfPuerperal":
                currentList = data.wolfPuerperal;
                title = "Wolf Puerperal";
                break;
            case "ChickenSpeed":
                currentList = data.chickenSpeed;
                title = "Chicken Speed";
                break;
            case "ChickenStrength":
                currentList = data.chickenStrength;
                title = "Chicken Strength";
                break;
            case "ChickenVision":
                currentList = data.chickenVision;
                title = "Chicken Vision";
                break;
            case "ChickenEnergy":
                currentList = data.chickenEnergy;
                title = "Chicken Energy";
                break;
            case "ChickenPuerperal":
                currentList = data.chickenPuerperal;
                title = "Chicken Puerperal";
                break;
            default:
                break;
        }
        graph.ShowGraph(currentList, graph.lineGraphVisual);
        titleText.text = title;
    }
}
