using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public enum SpeciesType
    {
        Prey,
        Predator
    }
    
    public SpeciesType type;
    public int speed, health, vision, energy;
}
