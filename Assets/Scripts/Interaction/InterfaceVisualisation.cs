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
                    mainRenderer.material = uiExploration;
                    SetHUD(StringConstants.ExplorationModeInfo);
                    SetCenterText(StringConstants.ExplorationModeInfo);
                    break;
                case MenuMode.Selection:
                    mainRenderer.material = uiSelection;
                    SetHUD(StringConstants.SelectionModeInfo);
                    SetCenterText(StringConstants.SelectionModeInfo);
                    break;
                case MenuMode.Selected:
                    if (!isSnapshotSelected)
                    {
                        mainRenderer.material = uiSelected;
                    }
                    break;
                case MenuMode.Mapping:
                    mainRenderer.material = uiSelected;
                    break;
                default:
                    mainRenderer.material = uiMain;
                    break;
            }
        }
        
        private void SetCenterText(string text) => centerText.text = text;

        private void SetHUD(string text = "") => hud.text = text;
    }
}
