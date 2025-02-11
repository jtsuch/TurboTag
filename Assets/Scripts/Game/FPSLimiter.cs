using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    public float limit;

    void Awake()
    {
        Application.targetFrameRate = (int) limit;
    } 

    public void UpdateFPS()
    {
        Application.targetFrameRate = (int)limit;
    }
}
