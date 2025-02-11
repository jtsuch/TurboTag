using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class GeneralController : MonoBehaviour
{

    [Header("Script References")]
    public PlayerMovement pm;
    public FPSLimiter fl;

    [Header("Input References")]
    public Slider sensSlider;
    public TMP_InputField sensInput;
    public Slider fpsSlider;
    public TMP_InputField fpsInput;

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void QuitGame()
    {
        Debug.Log("HERE:");
        Application.Quit();
    }

    public void SliderSense(float value)
    {
        pm.sensitivity = value; // Change actual sense
        sensInput.text = "" + value; // Change input value
    }

    public void InputSense()
    {
        float newSens = float.Parse(sensInput.text);
        pm.sensitivity = newSens; // Change actual sense
        sensSlider.value = newSens; // Change slider value
    }

    public void SliderFPS(float value)
    {
        fl.limit = value; // Change actual FPS
        fpsInput.text = "" + value; // Change input value
        fl.UpdateFPS(); // Actually update the FPS
    }

    public void InputFPS()
    {
        float newFPS = float.Parse(fpsInput.text);
        fl.limit = newFPS; // Change actual FPS
        fpsSlider.value = newFPS; // Change slider value
        fl.UpdateFPS(); // Actually update the FPS
    }

}
