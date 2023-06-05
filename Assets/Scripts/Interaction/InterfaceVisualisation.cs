using Constants;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction
{
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

        private void Awake()
        {
            ui_main = Resources.Load(StringConstants.MaterialUIMain, typeof(Material)) as Material;
            ui_exploration = Resources.Load(StringConstants.MaterialUIExploration, typeof(Material)) as Material;
            ui_selection = Resources.Load(StringConstants.MaterialUISelection, typeof(Material)) as Material;
            ui_selected = Resources.Load(StringConstants.MaterialUISelected, typeof(Material)) as Material;
        }

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
                    mainRenderer.material = ui_exploration;
                    SetHUD(StringConstants.ExplorationModeInfo);
                    SetCenterText(StringConstants.ExplorationModeInfo);
                    break;
                case MenuMode.Selection:
                    mainRenderer.material = ui_selection;
                    SetHUD(StringConstants.SelectionModeInfo);
                    SetCenterText(StringConstants.SelectionModeInfo);
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
        
        private void SetCenterText(string text) => centerText.text = text;

        private void SetHUD(string text = "") => hud.text = text;
    }
}
