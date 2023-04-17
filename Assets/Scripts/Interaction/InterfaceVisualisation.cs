using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu class
/// Toggle between menu and detail view
/// </summary>
public class InterfaceVisualisation : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer mainRenderer;
    [SerializeField]
    private TextMeshProUGUI hud;
    [SerializeField]
    private Text centerText;
    
    private Material ui_main;
    private Material ui_exploration;
    private Material ui_selection;
    private Material ui_selected;

    private void Start()
    {
        ui_main = Resources.Load(StringConstants.MaterialUIMain, typeof(Material)) as Material;
        ui_exploration = Resources.Load(StringConstants.MaterialUIExploration, typeof(Material)) as Material;
        ui_selection = Resources.Load(StringConstants.MaterialUISelection, typeof(Material)) as Material;
        ui_selected = Resources.Load(StringConstants.MaterialUISelected, typeof(Material)) as Material;

        SetMode(MenuMode.None);
    }

    public void SetCenterText(string text) => centerText.text = text;

    public void SetHUD(string text = "") => hud.text = text;

    public void SetMode(MenuMode mode, bool isSnapshotSelected = false)
    {
        switch (mode)
        {
            case MenuMode.Analysis:
                mainRenderer.material = ui_exploration;
                break;
            case MenuMode.Selection:
                mainRenderer.material = ui_selection;
                break;
            case MenuMode.Selected:
                if (!isSnapshotSelected)
                {
                    mainRenderer.material = ui_selected;
                }
                break;
            case MenuMode.Mapping:
                mainRenderer.material = ui_selected;
                break;
            default:
                mainRenderer.material = ui_main;
                break;
        }
    }
}
