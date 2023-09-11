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
        
        private const float MinInputInterval = 0.2f; // 0.2sec - to avoid detecting multiple shakes per shake
        private int _shakeCounter;

        private InputTracker _shakeTracker;
        private InputTracker _tiltTracker;

        private Gyroscope _deviceGyroscope;

        private void Start()
        {
            _shakeTracker = new InputTracker();
            _shakeTracker.Threshold = 5f;

            _tiltTracker = new InputTracker();
            _tiltTracker.Threshold = 1.3f;
            _tiltTracker.TimeSinceLast = Time.unscaledTime;
            _deviceGyroscope = Input.gyro;
            _deviceGyroscope.enabled = true;
        }

        private void Update()
        {
            if (_shakeCounter > 0 && Time.unscaledTime > _shakeTracker.TimeSinceFirst + MinInputInterval * 5)
            {
                HandleShakeInput();
                _shakeCounter = 0;
            }        

            CheckShakeInput();
            CheckTiltInput();
        }

        private void CheckShakeInput()
        {
            if (Input.acceleration.sqrMagnitude >= _shakeTracker.Threshold
                && Time.unscaledTime >= _shakeTracker.TimeSinceLast + MinInputInterval)
            {
                _shakeTracker.TimeSinceLast = Time.unscaledTime;

                if (_shakeCounter == 0)
                {
                    _shakeTracker.TimeSinceFirst = _shakeTracker.TimeSinceLast;
                }

                _shakeCounter++;
            }
        }

        private void HandleShakeInput()
        {
            _shakeTracker.TimeSinceLast = Time.unscaledTime;
            client.SendShakeMessage(_shakeCounter);
        }

        /// <summary>
        /// https://docs.unity3d.com/2019.4/Documentation/ScriptReference/Input-gyro.html
        /// https://answers.unity.com/questions/1284652/inputgyroattitude-returns-zero-values-when-tested.html
        /// attitude does not work on all tablets / samsung galaxy s6 tab
        /// </summary>
        private void CheckTiltInput()
        {
            if (Time.unscaledTime >= _tiltTracker.TimeSinceLast + MinInputInterval * 5)
            {
                var horizontalTilt = _deviceGyroscope.rotationRateUnbiased.y; 

                if (Math.Abs(horizontalTilt) < _tiltTracker.Threshold)
                {
                    return;
                }

                _tiltTracker.TimeSinceLast = Time.unscaledTime;

                client.SendTiltMessage(horizontalTilt > 0);
            }
        }
    }
}
