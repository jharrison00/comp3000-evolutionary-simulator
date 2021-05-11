using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderToText : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;

    public void DisplayValueOfSlider()
    {
        text.text = slider.value.ToString();
    }
}
