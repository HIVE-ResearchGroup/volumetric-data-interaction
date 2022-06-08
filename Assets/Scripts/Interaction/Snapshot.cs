using UnityEngine;

public class Snapshot : MonoBehaviour
{
    public GameObject Viewer;
    public bool IsLookingAt = true;
    public GameObject OriginPlane;

    private void Update()
    {
        if (IsLookingAt)
        {
            gameObject.transform.LookAt(Viewer.transform);
            transform.forward = -transform.forward; //need to adjust as quad is else not visible
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SetSelected(true, true);
    }

    private void OnTriggerExit(Collider other)
    {
        SetSelected(false, true);
    }

    public void SetSelected(bool isSelected, bool isTrigger = false)
    {
        if (OriginPlane)
        {
            OriginPlane.SetActive(isSelected);
        }
    }
}