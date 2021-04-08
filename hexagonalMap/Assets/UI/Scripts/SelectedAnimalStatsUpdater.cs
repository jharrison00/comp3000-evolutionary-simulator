using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectedAnimalStatsUpdater : MonoBehaviour
{
    public GameObject animalPanel;

    public TMP_InputField type;
    public TextMeshProUGUI age, hunger, babies, speed, strength, vision, energy, puerperal;

    public static SelectedAnimalStatsUpdater Instance;
    public static bool isTextSelected = false;

    private GameObject animalObject;
    private Animal animal;
    private Animal currentAnimal;

    public void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        if (animal != null)
        {
            if (currentAnimal != animal && currentAnimal != null)
            {
                type.text = animal.name;
            }
            currentAnimal = animal;

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
        animal = animalObject.GetComponent<Animal>();
        animalPanel.SetActive(true);
    }

    public void RemovePanel()
    {
        animalPanel.SetActive(false);
        animalObject = null;
    }

    public void UpdateType(string newType)
    {
        type.text = newType;
    }

    public void Selected()
    {
        isTextSelected = true;
    }

    public void Deselected()
    {
        isTextSelected = false;
    }
}
