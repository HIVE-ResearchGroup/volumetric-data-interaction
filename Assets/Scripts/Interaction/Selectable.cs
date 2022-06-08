using UnityEditor;
using UnityEngine;

/// <summary>
/// Halo hack: https://answers.unity.com/questions/10534/changing-color-of-halo.html
/// </summary>
public class Selectable : MonoBehaviour
{
    //private Material highlightedMaterial;
    //private Material defaultMaterial;
    private SerializedObject halo;

    private Host host;
    private bool isHighlighted = false;

    private void Start()
    {
        //highlightedMaterial = Resources.Load(StringConstants.MaterialYellowHighlighted, typeof(Material)) as Material;
        //defaultMaterial = gameObject.GetComponent<MeshRenderer>().material;

        host = FindObjectOfType<Host>();
        halo = new SerializedObject(gameObject.GetComponent(StringConstants.Halo)); // TODO fixproblem when creating new snapshots!
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
        //gameObject.GetComponent<MeshRenderer>().material = highlightedMaterial;

        halo.FindProperty("m_Enabled").boolValue = true;
        halo.ApplyModifiedProperties();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isHighlighted)
        {
            return;
        }

        isHighlighted = false;
        host.HighlightedObject = null;
        //gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;

        halo.FindProperty("m_Enabled").boolValue = false;
        halo.ApplyModifiedProperties();
    }

    public void SetToDefault()
    {
        isHighlighted = false;
        //gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
        halo.FindProperty("m_Color").colorValue = Color.yellow;
        halo.FindProperty("m_Enabled").boolValue = false;
        halo.ApplyModifiedProperties();
    }

    public void SetToSelected()
    {
        //var greenMaterial = Resources.Load(StringConstants.MateriaGreen, typeof(Material)) as Material;
        //gameObject.GetComponent<MeshRenderer>().material = greenMaterial;
        halo.FindProperty("m_Color").colorValue = Color.green;
        halo.ApplyModifiedProperties();
    }
}