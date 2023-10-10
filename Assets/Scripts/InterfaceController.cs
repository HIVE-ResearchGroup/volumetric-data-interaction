using Constants;
using Snapshots;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu class
/// Toggle between menu and detail view
/// </summary>
public class InterfaceController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI hud;
    
    [SerializeField]
    private Text centerText;

    [SerializeField]
    private Material uiMain;
    
    [SerializeField]
    private Material uiExploration;
    
    [SerializeField]
    private Material uiSelection;
    
    [SerializeField]
    private Material uiSelected;

    private void OnEnable()
    {
        SetMode(MenuMode.None);
    }

    public void SetMode(MenuMode mode, bool isSnapshotSelected = false)
    {
        switch (mode)
        {
            case MenuMode.None:
                SetHUD(StringConstants.MainModeInfo);
                SetCenterText(StringConstants.MainModeInfo);
                break;
            case MenuMode.Analysis:
                SnapshotManager.Instance.TabletOverlay.SetMaterial(uiExploration);
                SetHUD(StringConstants.ExplorationModeInfo);
                SetCenterText(StringConstants.ExplorationModeInfo);
                break;
            case MenuMode.Selection:
                SnapshotManager.Instance.TabletOverlay.SetMaterial(uiSelection);
                SetHUD(StringConstants.SelectionModeInfo);
                SetCenterText(StringConstants.SelectionModeInfo);
                break;
            case MenuMode.Selected:
                if (!isSnapshotSelected)
                {
                    SnapshotManager.Instance.TabletOverlay.SetMaterial(uiSelected);
                }
                break;
            case MenuMode.Mapping:
                SnapshotManager.Instance.TabletOverlay.SetMaterial(uiSelected);
                break;
            default:
                SnapshotManager.Instance.TabletOverlay.SetMaterial(uiMain);
                break;
        }
    }

    public void Unselect() => SnapshotManager.Instance.TabletOverlay.SetMaterial(null);
    
    private void SetCenterText(string text) => centerText.text = text;

    private void SetHUD(string text = "") => hud.text = text;
}
