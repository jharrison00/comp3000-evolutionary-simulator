using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectedAnimalStatsUpdater : MonoBehaviour
{
    public GameObject animalPanel;
    public TextMeshProUGUI type, age, hunger, babies, speed, strength, vision, energy, puerperal;

    public static SelectedAnimalStatsUpdater Instance;

    private GameObject animalObject;
    private Animal animal;

    public void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        if (animal != null)
        {
            if (animal.name.Contains("Chicken")) 
                type.text = "Chicken";
            else if (animal.name.Contains("Chick"))
                type.text = "Chick";
            else if (animal.name.Contains("Wolf"))
                type.text = "Wolf";
            else if (animal.name.Contains("Pup"))
                type.text = "Pup";

            age.text = "Age " + animal.age;
            hunger.text = "Hunger " + animal.hunger;
            babies.text = "Babies " + animal.babies;
            speed.text = "Speed " + animal.speed;
            strength.text = "Strength " + animal.strength;
            vision.text = "Vision " + animal.vision;
            energy.text = "Energy " + animal.energy; 
            puerperal.text = "Puerperal " + animal.puerperal;
        }
        else
        {
            RemovePanel();
        }
    }
    public void SetPanel(GameObject animalObject)
    {
        this.animalObject = animalObject;
        this.animal = animalObject.GetComponent<Animal>();
        animalPanel.SetActive(true);
    }

    public void RemovePanel()
    {
        animalPanel.SetActive(false);
        this.animalObject = null;
    }
}
