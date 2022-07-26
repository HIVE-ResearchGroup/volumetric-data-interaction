using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu class
/// Toggle between menu and detail view
/// </summary>
public class InterfaceVisualisation : MonoBehaviour
{
    private TextMeshProUGUI hud;
    private Text centerText;

    private Material ui_main;
    private Material ui_exploration;
    private Material ui_selection;
    private Material ui_selected;

    private GameObject mainOverlay;

    private void Start()
    {
        hud = GameObject.Find(StringConstants.HudText).GetComponent<TextMeshProUGUI>(); 
        centerText = GameObject.Find(StringConstants.CenterText).GetComponent<Text>(); 

        ui_main = Resources.Load(StringConstants.MaterialUIMain, typeof(Material)) as Material;
        ui_exploration = Resources.Load(StringConstants.MaterialUIExploration, typeof(Material)) as Material;
        ui_selection = Resources.Load(StringConstants.MaterialUISelection, typeof(Material)) as Material;
        ui_selected = Resources.Load(StringConstants.MaterialUISelected, typeof(Material)) as Material;

        mainOverlay = GameObject.Find(StringConstants.Main);
        SetMode(MenuMode.None);
    }

    public void SetCenterText(string text)
    {
        centerText.text = text;
    }

    public void SetHUD(string text = "")
    {
        hud.text = text;
    }

    public void SetMode(MenuMode mode, bool isSnapshotSelected = false)
    {
        switch (mode)
        {
            case MenuMode.Analysis:
                mainOverlay.GetComponent<MeshRenderer>().material = ui_exploration;
                break;
            case MenuMode.Selection:
                mainOverlay.GetComponent<MeshRenderer>().material = ui_selection;
                break;
            case MenuMode.Selected:
                if (!isSnapshotSelected)
                {
                    mainOverlay.GetComponent<MeshRenderer>().material = ui_selected;
                }
                break;
            case MenuMode.Mapping:
                mainOverlay.GetComponent<MeshRenderer>().material = ui_selected;
                break;
            default:
                mainOverlay.GetComponent<MeshRenderer>().material = ui_main;
                break;
        }
    }
}
