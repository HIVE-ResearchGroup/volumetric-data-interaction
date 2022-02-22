using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu class
/// Toggle between menu and detail view
/// Allow button interaction
/// Detect Shake: https://resocoder.com/2018/07/20/shake-detecion-for-mobile-devices-in-unity-android-ios/
/// </summary>
public class Menu : MonoBehaviour
{
    public Text Log;
    public float ShakeDetectionThreshold; // 3.6
    public float MinShakeInterval; // 0.2sec - to avoid detecting multiple shakes per shakel
    
    private float maxMultiShakeInterval = 1.5f; // 1.5 sec to do multiple shakes
    private float sqrShakeDetectionThreshold;
    private int shakeCounter;
    private float timeSinceLastShake;
    private float timeSinceFirstShake;

    void Start()
    {
        sqrShakeDetectionThreshold = Mathf.Pow(ShakeDetectionThreshold, 2);
    }

    void Update()
    {
        if (shakeCounter > 0 && Time.unscaledTime > timeSinceFirstShake + maxMultiShakeInterval)
        {
            HandleShakeInput(shakeCounter);
            shakeCounter = 0;
        }

        //Debug Input
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMenu();
        }
        else if (Input.GetKeyDown(KeyCode.L)) {
            timeSinceLastShake = Time.unscaledTime;

            if (shakeCounter == 0)
                timeSinceFirstShake = timeSinceLastShake;

            shakeCounter++;
        }

        CheckShakeInput();
    }

    private void CheckShakeInput()
    {
        if (Input.acceleration.sqrMagnitude >= sqrShakeDetectionThreshold
            && Time.unscaledTime >= timeSinceLastShake + MinShakeInterval)
        {
            timeSinceLastShake = Time.unscaledTime;

            if (shakeCounter == 0)
            {
                timeSinceFirstShake = timeSinceLastShake;
            }

            shakeCounter++;
        }
    }

    private void HandleShakeInput(int shakeCounter)
    {
        timeSinceLastShake = Time.unscaledTime;

        if (shakeCounter == 1)
        {
            Log.text = "Single Shake"; // TODO - align
        }
        else if (shakeCounter > 1)
        {
            Log.text = "Multiple Shakes"; // TODO - reset
        }
    }


    /// <summary>
    /// Display menu on plane
    /// </summary>
    public void ToggleMenu()
    {

    }
}
