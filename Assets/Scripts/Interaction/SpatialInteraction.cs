using System;
using UnityEngine;

namespace Interaction
{
    public class SpatialInteraction : MonoBehaviour
    {
        [SerializeField]
        private Transform tracker;

        private Quaternion _startInputRotation;
        private Quaternion _startRotation;
        private bool _rotationMapping;

        private bool _updateStartRotation;

        //private Vector3 _startInputTransform;
        private Vector3 _startTransform;
        private Vector3 _calibrationResult;
        private Vector3 _velocity = Vector3.zero;
        private Vector3 _accumulatedTransformation;
        private bool _transformMapping;

        //private bool _updateStartTransform;

        public bool RotationMapping
        {
            get => _rotationMapping;
            set
            {
                if (value == _rotationMapping)
                {
                    return;
                }
                _rotationMapping = value;
                if (_rotationMapping)
                {
                    _updateStartRotation = true;
                }
            }
        }

        public bool TransformMapping
        {
            get => _transformMapping;
            set
            {
                /*if (value == _transformMapping)
                {
                    return;
                }*/
                _transformMapping = value;
                /*if (_transformMapping)
                {
                    _updateStartTransform = true;
                }*/
            }
        }

        /// <summary>
        /// Execute rotation depending on tracker orientation 
        /// </summary>
        public void HandleRotation(float rotation, GameObject selectedObject)
        {
            if (!selectedObject)
            {
                return;
            }

            var trackerTransform = tracker.transform;
            var threshold = 20.0f;
            var downAngle = 90.0f;

            if (trackerTransform.eulerAngles.x <= downAngle + threshold && trackerTransform.eulerAngles.x >= downAngle - threshold)
            {
                selectedObject.transform.Rotate(0.0f, rotation * Mathf.Rad2Deg, 0.0f);
                return;
            }

            if (trackerTransform.rotation.x <= 30f && 0f <= trackerTransform.rotation.x ||
                trackerTransform.rotation.x <= 160f && 140f <= trackerTransform.rotation.x)
            {
                selectedObject.transform.Rotate(Vector3.up, -rotation * Mathf.Rad2Deg);
            }
            else
            {
                selectedObject.transform.Rotate(Vector3.forward, rotation * Mathf.Rad2Deg);
            }
        }

        public void HandleRotation(Quaternion rotation, GameObject selectedObject)
        {
            if (!RotationMapping ||
                selectedObject == null)
            {
                return;
            }

            if (_updateStartRotation)
            {
                _updateStartRotation = false;
                _startRotation = selectedObject.transform.rotation;
                _startInputRotation = rotation;
            }

            Debug.Log($"Rotation: {rotation}");
            selectedObject.transform.rotation = _startRotation * (rotation * Quaternion.Inverse(_startInputRotation));
        }

        public void HandleTransform(Vector3 transformation, GameObject selectedObject)
        {
            /*if (!TransformMapping ||
                selectedObject == null)
            {
                return;
            }

            if (_updateStartTransform)
            {
                _updateStartTransform = false;
                _startTransform = selectedObject.transform.position;
                //_startInputTransform = transformation;
                _velocity = Vector3.zero;
                _calibrationResult = transformation;
                _accumulatedTransformation = Vector3.zero;
            }

            // 1) get acceleration
            // 2) get current velocity
            // 3) apply velocity to get a new position
            // keep it relatively smooth, as the sensors are twitchy
            var acceleration = transformation - _calibrationResult;
            if (acceleration.magnitude <= 0.01f)
            {
                acceleration = Vector3.zero;
            }
            _velocity += acceleration;
            if (_velocity.magnitude <= 0.01f)
            {
                _velocity = Vector3.zero;
            }
            _accumulatedTransformation += _velocity;
            _accumulatedTransformation.x = Mathf.Round(_accumulatedTransformation.x * 100.0f) / 100.0f;
            _accumulatedTransformation.y = Mathf.Round(_accumulatedTransformation.y * 100.0f) / 100.0f;
            _accumulatedTransformation.z = Mathf.Round(_accumulatedTransformation.z * 100.0f) / 100.0f;
            Debug.Log($"Input Transformation: {transformation}");
            Debug.Log($"Velocity: {_velocity}");
            Debug.Log($"Accumulated Transformation: {_accumulatedTransformation}");
            selectedObject.transform.position = _startTransform + _accumulatedTransformation;
            //selectedObject.transform.Translate(transformation, Space.World);
            */
        }

        /*public void StartMapping(GameObject selectedObject)
        {
            if (selectedObject is null)
            {
                return;
            }

            _mapping = true;

            //StartCoroutine(nameof(MapObject), selectedObject);
        }

        public void StopMapping(GameObject selectedObject)
        {
            if (selectedObject is null)
            {
                return;
            }

            _mapping = false;

            //StopCoroutine(nameof(MapObject));
            //if (selectedObject.TryGetComponent(out Selectable s))
            //{
            //    s.Freeze();
            //}
        }

        private IEnumerator MapObject(GameObject selectedObject)
        {
            var prevPos = tracker.position;

            var rotationOffset = Quaternion.Inverse(tracker.rotation) * selectedObject.transform.rotation;
            while (true)
            {
                var currPos = tracker.position;

                selectedObject.transform.position += currPos - prevPos;
                selectedObject.transform.rotation = tracker.rotation * rotationOffset;

                prevPos = currPos;
                yield return null;
            }
        }*/
    }
}