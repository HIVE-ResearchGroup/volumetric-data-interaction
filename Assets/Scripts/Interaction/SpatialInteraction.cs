using System.Collections;
using UnityEngine;

namespace Interaction
{
    public class SpatialInteraction : MonoBehaviour
    {
        [SerializeField]
        private Transform tracker;

        private bool _mapping;

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

        public void HandleRotation(Vector3 rotation, GameObject selectedObject)
        {
            if (!_mapping
                || selectedObject is null)
            {
                return;
            }

            Debug.Log($"Rotation: {rotation}");
            selectedObject.transform.Rotate(-rotation.x, -rotation.y, rotation.z, Space.World);
        }

        public void StartMapping(GameObject selectedObject)
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

            /*StopCoroutine(nameof(MapObject));
            if (selectedObject.TryGetComponent(out Selectable s))
            {
                s.Freeze();
            }*/
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