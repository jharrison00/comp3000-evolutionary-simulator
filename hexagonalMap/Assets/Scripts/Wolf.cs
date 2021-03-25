using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Animal
{
    public WolvesController wolfController = WolvesController.Instance;

    private bool eating = false;
    private bool timerActive = false;
    private float timer = 0;

    public Wolf mateCallSent;
    public Wolf mateCallRecieved;

}
