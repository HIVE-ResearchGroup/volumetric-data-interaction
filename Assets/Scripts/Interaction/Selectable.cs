using UnityEngine;

public class Selectable : MonoBehaviour
{
    private Material highlightedMaterial;
    private Material defaultMaterial;

    private Host host;
    private bool isHighlighted = false;

    private void Start()
    {
        highlightedMaterial = Resources.Load(StringConstants.MaterialYellowHighlighted, typeof(Material)) as Material;
        defaultMaterial = gameObject.GetComponent<MeshRenderer>().material;

        host = FindObjectOfType<Host>();
    }

    /// <summary>
    /// Selecteables are only highlighted if there is not already a highlighted object marked as selected in host script
    /// This should avoid selection overlap which could occur with overlapping objects
    /// The first to be selected is the only to be selected
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (host.HighlightedObject != null)
        {
            return;
        }

        isHighlighted = true;
        host.HighlightedObject = gameObject;
        gameObject.GetComponent<MeshRenderer>().material = highlightedMaterial;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isHighlighted)
        {
            return;
        }

        isHighlighted = false;
        host.HighlightedObject = null;
        gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
    }

    public void SetToDefault()
    {
        isHighlighted = false;
        gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
    }
}