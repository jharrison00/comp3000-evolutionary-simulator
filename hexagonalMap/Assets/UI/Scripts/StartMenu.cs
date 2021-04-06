using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StartMenu : MonoBehaviour
{
    public Toggle small, medium, large;
    public Slider foodSlider;
    public Slider chickenAmountSlider, chickenMutationSlider, chickenSpeedSlider, chickenStrengthSlider, chickenVisionSlider, chickenEnergySlider, chickenPuerperalSlider;
    public Slider wolfAmountSlider, wolfMutationSlider, wolfSpeedSlider, wolfStrengthSlider, wolfVisionSlider, wolfEnergySlider, wolfPuerperalSlider;

    public void PlayGame()
    {
        if (small.isOn)
        {
            PlayerPrefs.SetInt("size", 21);
        }
        else if (medium.isOn)
        {
            PlayerPrefs.SetInt("size", 31);
        }
        else if (large.isOn)
        {
            PlayerPrefs.SetInt("size", 51);
        }

        PlayerPrefs.SetInt("foodPercent", (int)foodSlider.value);

        PlayerPrefs.SetInt("chickenAmount", (int)chickenAmountSlider.value);
        PlayerPrefs.SetInt("chickenMutation", (int)chickenMutationSlider.value);
        PlayerPrefs.SetInt("chickenSpeed", (int)chickenSpeedSlider.value);
        PlayerPrefs.SetInt("chickenStrength", (int)chickenStrengthSlider.value);
        PlayerPrefs.SetInt("chickenVision", (int)chickenVisionSlider.value);
        PlayerPrefs.SetInt("chickenEnergy", (int)chickenEnergySlider.value);
        PlayerPrefs.SetInt("chickenPuerperal", (int)chickenPuerperalSlider.value);

        PlayerPrefs.SetInt("wolfAmount", (int)wolfAmountSlider.value);
        PlayerPrefs.SetInt("wolfMutation", (int)wolfMutationSlider.value);
        PlayerPrefs.SetInt("wolfSpeed", (int)wolfSpeedSlider.value);
        PlayerPrefs.SetInt("wolfStrength", (int)wolfStrengthSlider.value);
        PlayerPrefs.SetInt("wolfVision", (int)wolfVisionSlider.value);
        PlayerPrefs.SetInt("wolfEnergy", (int)wolfEnergySlider.value);
        PlayerPrefs.SetInt("wolfPuerperal", (int)wolfPuerperalSlider.value);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
