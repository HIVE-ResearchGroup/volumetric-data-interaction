using UnityEngine;

/// <summary>
/// Added to ray, will colour ray on collision with object
/// </summary>
public class Selector : MonoBehaviour
{
    public bool HasCollision;

    private Material defaultSelectorMaterial;
    private Material highlightedSelectorMaterial;

    private void Start()
    {
        defaultSelectorMaterial = Resources.Load(StringConstants.MaterialYellow, typeof(Material)) as Material;
        highlightedSelectorMaterial = Resources.Load(StringConstants.MaterialYellowHighlighted, typeof(Material)) as Material;
        HasCollision = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        gameObject.GetComponent<MeshRenderer>().material = highlightedSelectorMaterial;
        HasCollision = true;
    }

    private void OnTriggerExit(Collider other)
    {
        gameObject.GetComponent<MeshRenderer>().material = defaultSelectorMaterial;
        HasCollision = false;
    }
}