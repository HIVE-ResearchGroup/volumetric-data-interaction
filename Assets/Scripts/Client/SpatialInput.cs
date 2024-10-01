#nullable enable

using System;
using UnityEngine;

namespace Client
{
    /// <summary>
    /// Detect Shake: https://resocoder.com/2018/07/20/shake-detecion-for-mobile-devices-in-unity-android-ios/
    /// To use the class Gyroscope, the device needs to have a gyroscope
    /// </summary>
    public class SpatialInput : MonoBehaviour
    {
        public event Action<int>? Shook;
        
        private const float MinInputInterval = 0.2f; // 0.2sec - to avoid detecting multiple shakes per shake

        private int _shakeCounter;

        private const float _shakeThreshold = 5.0f;
        private float _shakeTimeSinceFirst;
        private float _shakeTimeSinceLast;

        private Gyroscope _deviceGyroscope = null!;

        private void Start()
        {
            _deviceGyroscope = Input.gyro;
            _deviceGyroscope.enabled = true;
        }

        private void Update()
        {
            if (_shakeCounter > 0 && Time.unscaledTime > _shakeTimeSinceFirst + MinInputInterval * 5)
            {
                _shakeTimeSinceLast = Time.unscaledTime;
                Shook?.Invoke(_shakeCounter);
                _shakeCounter = 0;
            }

            if (Input.acceleration.sqrMagnitude >= _shakeThreshold
                && Time.unscaledTime >= _shakeTimeSinceLast + MinInputInterval)
            {
                _shakeTimeSinceLast = Time.unscaledTime;

                if (_shakeCounter == 0)
                {
                    _shakeTimeSinceFirst = _shakeTimeSinceLast;
                }

                _shakeCounter++;
            }
        }
    }
}
