using System;
using Constants;
using UnityEngine;
using NetworkOld;
using NetworkOld.Message;

namespace Interaction
{
    /// <summary>
    /// Detect Shake: https://resocoder.com/2018/07/20/shake-detecion-for-mobile-devices-in-unity-android-ios/
    /// To use the class Gyroscope, the device needs to have a gyroscope
    /// </summary>
    public class SpatialInput : MonoBehaviour
    {
        private float minInputInterval = 0.2f; // 0.2sec - to avoid detecting multiple shakes per shake
        private int shakeCounter;

        private InputTracker shakeTracker;
        private InputTracker tiltTracker;

        private Gyroscope deviceGyroscope;

        void Start()
        {
            shakeTracker = new InputTracker();
            shakeTracker.Threshold = 5f;

            tiltTracker = new InputTracker();
            tiltTracker.Threshold = 1.3f;
            tiltTracker.TimeSinceLast = Time.unscaledTime;
            deviceGyroscope = Input.gyro;
            deviceGyroscope.enabled = true;
        }

        void Update()
        {
            if (shakeCounter > 0 && Time.unscaledTime > shakeTracker.TimeSinceFirst + minInputInterval * 5)
            {
                HandleShakeInput(shakeCounter);
                shakeCounter = 0;
            }        

            CheckShakeInput();
            CheckTiltInput();
        }

        private void SendToHost(NetworkMessage message)
        {
            var client = GameObject.Find(StringConstants.Client)?.GetComponent<Client>();
            client?.SendServer(message);
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

        private void HandleShakeInput(int shakeCounter)
        {
            shakeTracker.TimeSinceLast = Time.unscaledTime;
            SendToHost(new ShakeMessage(shakeCounter));
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

                SendToHost(new TiltMessage(horizontalTilt > 0));
            }                
        }
    }
}
