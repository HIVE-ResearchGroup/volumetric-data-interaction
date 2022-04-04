using UnityEngine;

public class Viewable : MonoBehaviour
{
    public GameObject Viewer;

    private void Update()
    {
        gameObject.transform.LookAt(Viewer.transform);
    }
}