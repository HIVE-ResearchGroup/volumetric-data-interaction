using UnityEngine;

/// <summary>
/// Halo hack: https://answers.unity.com/questions/10534/changing-color-of-halo.html
/// </summary>
public class Selectable : MonoBehaviour
{
    private Material greenMaterial;
    private Material highlightedMaterial;
    private Material defaultMaterial;

    private Host host;
    private bool isHighlighted = false;
    private MeshRenderer renderer;

    private void Start()
    {
        highlightedMaterial = Resources.Load(StringConstants.MaterialYellowHighlighted, typeof(Material)) as Material;
        greenMaterial = Resources.Load(StringConstants.MaterialGreen, typeof(Material)) as Material;

        renderer = gameObject.GetComponent<MeshRenderer>();
        defaultMaterial = renderer.material;
        host = FindObjectOfType<Host>();
    }

    /// <summary>
    /// Selecteables are only highlighted if there is not already a highlighted object marked as selected in host script
    /// This should avoid selection overlap which could occur with overlapping objects
    /// The first to be selected is the only to be selected
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (host.HighlightedObject != null || !other.name.Contains(StringConstants.Ray))
        {
            return;
        }

        isHighlighted = true;
        host.HighlightedObject = gameObject;
        SetMaterial(highlightedMaterial);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isHighlighted)
        {
            return;
        }

        isHighlighted = false;
        host.HighlightedObject = null;
        renderer.material = defaultMaterial;
    }

    public void SetToDefault()
    {
        isHighlighted = false;
        renderer.material = defaultMaterial;
        Freeze();
    }

    public void SetToSelected()
    {
        SetMaterial(greenMaterial);
    }

    public void Freeze()
    {
        var rigidbody = gameObject.GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void UnFreeze()
    {
        var rigidbody = gameObject.GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.None;
    }

    private void SetMaterial(Material newMaterial)
    {
        renderer.material = newMaterial;
        renderer.material.mainTexture = defaultMaterial.mainTexture;
        renderer.material.mainTextureScale = defaultMaterial.mainTextureScale;
    }
}