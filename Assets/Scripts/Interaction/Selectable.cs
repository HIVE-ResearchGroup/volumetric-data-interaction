using UnityEditor;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    private Host host;
    private bool isHighlighted = false;
    private SerializedObject halo;

    private void Start()
    {
        halo = new SerializedObject(gameObject.GetComponent(StringConstants.Halo));
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

        halo.FindProperty("m_Enabled").boolValue = true;
        halo.FindProperty("m_Color").colorValue = Color.yellow;
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
        halo.FindProperty("m_Enabled").boolValue = false;
        halo.ApplyModifiedProperties();
    }

    public void SetToDefault()
    {
        isHighlighted = false;
        halo.FindProperty("m_Enabled").boolValue = false;
        halo.ApplyModifiedProperties();
    }

    public void SetToSelected()
    {
        halo.FindProperty("m_Enabled").boolValue = true;
        halo.FindProperty("m_Color").colorValue = Color.green;
        halo.ApplyModifiedProperties();
    }
}