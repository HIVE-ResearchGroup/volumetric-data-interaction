using System;
using System.Collections;
using Networking;
using UnityEngine;

namespace Interaction
{
    public class SpatialInteraction : MonoBehaviour
    {
        [SerializeField]
        private Transform tracker;
        [SerializeField]
        private Host host;
        [SerializeField]
        private NetworkingCommunicatorProxy netComm;

        private void Start()
        {
            netComm.Rotated += rotation => HandleRotation(rotation, host.Selected);
        }

        /// <summary>
        /// Execute rotation depending on tracker orientation 
        /// </summary>
        public void HandleRotation(float rotation, GameObject selectedObject)
        {
            Debug.Log(rotation);
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

        public void StartMapping(GameObject selectedObject)
        {
            if (selectedObject == null)
            {
                return;
            }

            StartCoroutine(nameof(MapObject), selectedObject);
        }

        public void StopMapping(GameObject selectedObject)
        {
            if (selectedObject == null)
            {
                return;
            }

            StopCoroutine(nameof(MapObject));
            if (selectedObject.TryGetComponent(out Selectable s))
            {
                s.Freeze();
            }
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
        }
    }
}