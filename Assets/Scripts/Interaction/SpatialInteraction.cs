using System.Collections;
using UnityEngine;

public class SpatialInteraction : MonoBehaviour
{
    public GameObject tracker;

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

    public void StartMapping(GameObject selectedObject)
    {
        if (selectedObject == null)
        {
            return;
        }

        StartCoroutine(StringConstants.MapObject, selectedObject);
    }

    public void StopMapping(GameObject selectedObject)
    {
        if (selectedObject == null)
        {
            return;
        }

        StopCoroutine(StringConstants.MapObject);
        selectedObject.GetComponent<Selectable>()?.Freeze();
    }

    private IEnumerator MapObject(GameObject selectedObject)
    {
        float currX, currY, currZ;

        var prevX = tracker.transform.position.x;
        var prevY = tracker.transform.position.y;
        var prevZ = tracker.transform.position.z;

        var rotationOffset = Quaternion.Inverse(tracker.transform.rotation) * selectedObject.transform.rotation;
        while (true)
        {
            currX = tracker.transform.position.x;
            currY = tracker.transform.position.y;
            currZ = tracker.transform.position.z;

            selectedObject.transform.position += new Vector3(currX - prevX, currY - prevY, currZ - prevZ);
            selectedObject.transform.rotation = tracker.transform.rotation * rotationOffset;

            prevX = tracker.transform.position.x;
            prevY = tracker.transform.position.y;
            prevZ = tracker.transform.position.z;
            yield return null;
        }
    }
}