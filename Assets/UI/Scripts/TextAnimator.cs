using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextAnimator : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float fontChangeSpeed = 2.5f;
    public float fontChangeSize = 1f;

    public float rotationSpeed = 2f;
    public float rotationChangeSize = 1f;

    private float initialTextSize;

    private bool increaseText = true;
    private bool increaseRotation = true;


    public void Start()
    {
        initialTextSize = text.fontSize;
    }

    public void Update()
    {
        float fontSize = text.fontSize;

        if (fontSize >= initialTextSize + fontChangeSize)  
            increaseText = false; 
        else if (fontSize <= initialTextSize - fontChangeSize) 
            increaseText = true;

        if (increaseText)
            fontSize += Time.deltaTime * fontChangeSpeed;
        else
            fontSize -= Time.deltaTime * fontChangeSpeed;

        text.fontSize = fontSize;

        float zRotation = text.rectTransform.localEulerAngles.z;
        zRotation = (zRotation > 180) ? zRotation - 360 : zRotation;

        if (zRotation >= rotationChangeSize) 
            increaseRotation = false;
        if (zRotation <= -rotationChangeSize)
            increaseRotation = true;

        if (increaseRotation)
            text.rectTransform.Rotate(Vector3.forward * (Time.deltaTime * rotationSpeed));
        else
            text.rectTransform.Rotate(Vector3.back * (Time.deltaTime * rotationSpeed));

    }
}
