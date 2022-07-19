using System.Collections;
using UnityEngine;

public class SpatialInteraction : MonoBehaviour
{
    public GameObject Tracker;

    /// <summary>
    /// Execute rotation depending on tracker orientation and position to object
    /// The position of the object is slightly extended to check which axis is closer to the tracker
    /// Problems could occur if the object has already been rotated
    /// If so, the axis do not align as they should
    /// </summary>
    public void HandleRotation(float rotation, GameObject selectedObject)
    {
        if (!selectedObject)
        {
            return;
        }

        var trackerTransform = Tracker.transform;
        var threshold = 20.0f;
        var downAngle = 90.0f;

        if (trackerTransform.eulerAngles.x <= downAngle + threshold && trackerTransform.eulerAngles.x >= downAngle - threshold)
        {
            selectedObject.transform.Rotate(0.0f, rotation * Mathf.Rad2Deg, 0.0f);
            return;
        }

        var distanceX = GetMinAxisDistance(1, 0, 0, selectedObject);
        var distanceY = GetMinAxisDistance(0, 1, 0, selectedObject);
        var distanceZ = GetMinAxisDistance(0, 0, 1, selectedObject);

        Debug.Log(distanceX + " - " + distanceY + " - " + distanceZ);
        if (distanceX <= distanceY && distanceX <= distanceZ)
        {
            selectedObject.transform.Rotate(rotation * Mathf.Rad2Deg, 0.0f, 0.0f);
        }
        else if (distanceY <= distanceX && distanceY <= distanceZ)
        {
            selectedObject.transform.Rotate(0.0f, 0.0f, rotation * Mathf.Rad2Deg);
        }
        else
        {
            selectedObject.transform.Rotate(0.0f, rotation * Mathf.Rad2Deg, 0.0f);
        }
    }

    private float GetMinAxisDistance(float x, float y, float z, GameObject selectedObject)
    {
        var trackerTransform = Tracker.transform;
        var objectTransform = selectedObject.transform;

        var extendedObjectPos = objectTransform.position + new Vector3(x, y, z);
        var extendedObjectNeg = objectTransform.position + new Vector3(-x, -y, -z);

        var distancePos = Vector3.Distance(trackerTransform.position, extendedObjectPos);
        var distanceNeg = Vector3.Distance(trackerTransform.position, extendedObjectNeg);

        return distanceNeg <= distancePos ? distanceNeg : distancePos;
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

        var prevX = Tracker.transform.position.x;
        var prevY = Tracker.transform.position.y;
        var prevZ = Tracker.transform.position.z;

        var rotationOffset = Tracker.transform.rotation * Quaternion.Inverse(selectedObject.transform.rotation);
        while (true)
        {
            currX = Tracker.transform.position.x;
            currY = Tracker.transform.position.y;
            currZ = Tracker.transform.position.z;

            selectedObject.transform.position += new Vector3(currX - prevX, currY - prevY, currZ - prevZ);
            selectedObject.transform.rotation = Tracker.transform.rotation * rotationOffset;

            prevX = Tracker.transform.position.x;
            prevY = Tracker.transform.position.y;
            prevZ = Tracker.transform.position.z;

            yield return null;
        }
    }
}