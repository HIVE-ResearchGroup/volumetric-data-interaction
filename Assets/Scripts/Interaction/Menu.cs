using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu class
/// Toggle between menu and detail view
/// Allow button interaction
/// Detect Shake: https://resocoder.com/2018/07/20/shake-detecion-for-mobile-devices-in-unity-android-ios/
/// To use the class Gyroscope, the device needs to have a gyroscope
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

    private float tiltThreshold = 0.1f;
    private Gyroscope deviceGyroscope;

    void Start()
    {
        sqrShakeDetectionThreshold = Mathf.Pow(ShakeDetectionThreshold, 2);

        deviceGyroscope = Input.gyro;
        deviceGyroscope.enabled = true;
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
        else if (Input.GetKeyDown(KeyCode.K))
        {
            Log.text = $"nothing yet";
        }

        CheckShakeInput();
        CheckTiltInput();
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
        var shakeMessage = new ShakeMessage();
        shakeMessage.Count = shakeCounter;

        var client = GameObject.Find(StringConstants.Client);

        if (client == null)
        {
            return;
        }

        client.GetComponent<Client>()?.SendServer(shakeMessage);
    }

    /// <summary>
    /// https://docs.unity3d.com/2019.4/Documentation/ScriptReference/Input-gyro.html
    /// </summary>
    private void CheckTiltInput()
    {
        var horizontalTilt = deviceGyroscope.attitude.y;
        
        if (Math.Abs(horizontalTilt) < tiltThreshold)
        {
            return;
        }

        if (horizontalTilt > 0) // TODO slice change
        {
            Log.text = "Tilt left";
        }
        else
        {
            Log.text = "Tilt right";
        }
    }

    public void DisplayData()
    {
        Log.text += $"Add log info for tablet";
    }

    public void RefreshData()
    {
        Log.text = "";
    }

    /// <summary>
    /// Display menu on plane
    /// </summary>
    public void ToggleMenu()
    {

    }
}
