using UnityEngine;

public class Viewable : MonoBehaviour
{
    public GameObject Viewer;
    public bool IsLookingAt = true;

    private void Update()
    {
        if (IsLookingAt)
        {
            gameObject.transform.LookAt(Viewer.transform);
            transform.forward = -transform.forward; //need to adjust as quad is else not visible
        }
    }
}