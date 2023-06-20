using System;
using Networking;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// Detect Shake: https://resocoder.com/2018/07/20/shake-detecion-for-mobile-devices-in-unity-android-ios/
    /// To use the class Gyroscope, the device needs to have a gyroscope
    /// </summary>
    public class SpatialInput : MonoBehaviour
    {
        [SerializeField]
        private Client client;
        
        private float minInputInterval = 0.2f; // 0.2sec - to avoid detecting multiple shakes per shake
        private int shakeCounter;

        private InputTracker shakeTracker;
        private InputTracker tiltTracker;

        private Gyroscope deviceGyroscope;

        private void Start()
        {
            shakeTracker = new InputTracker();
            shakeTracker.Threshold = 5f;

            tiltTracker = new InputTracker();
            tiltTracker.Threshold = 1.3f;
            tiltTracker.TimeSinceLast = Time.unscaledTime;
            deviceGyroscope = Input.gyro;
            deviceGyroscope.enabled = true;
        }

        private void Update()
        {
            if (shakeCounter > 0 && Time.unscaledTime > shakeTracker.TimeSinceFirst + minInputInterval * 5)
            {
                HandleShakeInput();
                shakeCounter = 0;
            }        

            CheckShakeInput();
            CheckTiltInput();
            CheckDeviceRotation();
            CheckDeviceMovement();
        }

        private void CheckShakeInput()
        {
            if (Input.acceleration.sqrMagnitude >= shakeTracker.Threshold
                && Time.unscaledTime >= shakeTracker.TimeSinceLast + minInputInterval)
            {
                shakeTracker.TimeSinceLast = Time.unscaledTime;

                if (shakeCounter == 0)
                {
                    shakeTracker.TimeSinceFirst = shakeTracker.TimeSinceLast;
                }

                shakeCounter++;
            }
        }

        private void HandleShakeInput()
        {
            shakeTracker.TimeSinceLast = Time.unscaledTime;
            client.HandleShakeMessage(shakeCounter);
        }

        /// <summary>
        /// https://docs.unity3d.com/2019.4/Documentation/ScriptReference/Input-gyro.html
        /// https://answers.unity.com/questions/1284652/inputgyroattitude-returns-zero-values-when-tested.html
        /// attitude does not work on all tablets / samsung galaxy s6 tab
        /// </summary>
        private void CheckTiltInput()
        {
            if (Time.unscaledTime >= tiltTracker.TimeSinceLast + minInputInterval * 5)
            {
                var horizontalTilt = deviceGyroscope.rotationRateUnbiased.y; 

                if (Math.Abs(horizontalTilt) < tiltTracker.Threshold)
                {
                    return;
                }

                tiltTracker.TimeSinceLast = Time.unscaledTime;

                client.HandleTiltMessage(horizontalTilt > 0);
            }
        }

        private void CheckDeviceRotation() => client.HandleRotateFullMessage(deviceGyroscope.attitude);

        private void CheckDeviceMovement()
        {
            var gravity = Vector3.Dot(deviceGyroscope.gravity, Vector3.up);
            client.HandleTransformMessage(new Vector3(Input.acceleration.x, Input.acceleration.y + gravity,
                Input.acceleration.z));
        }
    }
}
