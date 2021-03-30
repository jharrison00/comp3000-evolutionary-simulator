using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePanels : MonoBehaviour
{
    public GameObject overallPanel, chickenPanel, wolfPanel;

    public void TogglePanel()
    {
        bool isOverallActive = overallPanel.activeSelf;
        bool isChickenActive = chickenPanel.activeSelf;
        bool isWolfActive = wolfPanel.activeSelf;

        // If none are on - activate overall
        if (!isOverallActive && !isChickenActive && !isWolfActive) 
        {
            overallPanel.SetActive(!isOverallActive);
        }
        // If overall is on and not others - deactivate overall
        if (isOverallActive && !isChickenActive && !isWolfActive) 
        {
            overallPanel.SetActive(!isOverallActive);
        }
        // If chicken is on and not others - deactivate chicken
        if (!isOverallActive && isChickenActive && !isWolfActive)
        {
            chickenPanel.SetActive(!isChickenActive);
        }
        // If wolf is on and not others - deactivate wolf
        if (!isOverallActive && !isChickenActive && isWolfActive) 
        {
            wolfPanel.SetActive(!isWolfActive);
        }
    }
}
