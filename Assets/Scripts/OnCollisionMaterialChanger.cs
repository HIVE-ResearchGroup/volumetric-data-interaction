using UnityEngine;

public class OnCollisionMaterialChanger : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private Material defaultMaterial;

    [SerializeField]
    private Material highlightedMaterial;

    private void OnTriggerEnter(Collider other)
    {
        meshRenderer.material = highlightedMaterial;
    }

    private void OnTriggerExit(Collider other)
    {
        meshRenderer.material = defaultMaterial;
    }
}
