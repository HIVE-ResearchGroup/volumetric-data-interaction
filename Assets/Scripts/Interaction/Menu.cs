using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction
{
    /// <summary>
    /// Menu class
    /// Toggle between menu and detail view
    /// </summary>
    public class Menu : MonoBehaviour
    {
        [SerializeField]
        private Client client;
        [SerializeField]
        private GameObject mainMenu;
        [SerializeField]
        private GameObject interactionMenu;
        [SerializeField]
        private GameObject networkConfigMenu;
        [SerializeField]
        private Text headerText;

        private void OnEnable()
        {
            client.MenuModeChanged += HandleMenuModeChanged;
        }

        private void OnDisable()
        {
            client.MenuModeChanged -= HandleMenuModeChanged;
        }

        private void HandleMenuModeChanged(MenuMode mode)
        {
            switch (mode)
            {
                case MenuMode.Selected:
                    HandleObjectSelected();
                    break;
                case MenuMode.None:
                    Cancel();
                    break;
                default:
                    Debug.Log($"{nameof(HandleMenuModeChanged)} received unknown menu mode: {mode}");
                    break;
            }
        }

        private void HandleObjectSelected()
        {
            // set object as gameobject in a specific script?
            client.SendMenuChangedMessage(MenuMode.Selected);
        }
        
        public void SwitchToMainMenu()
        {
            mainMenu.SetActive(true);
            interactionMenu.SetActive(false);
            networkConfigMenu.SetActive(false);
        }
        
        public void StartSelection()
        {
            Debug.Log("Selection");
            client.SendMenuChangedMessage(MenuMode.Selection);
            SwitchToInteractionMenu("Selection Mode");
        }

        public void StartMapping() => client.SendMenuChangedMessage(MenuMode.Mapping);

        public void StopMapping()
        {
            client.SendTextMessage("Stopped mapping");
            HandleObjectSelected();
        }

        public void StartAnalysis()
        {
            client.SendMenuChangedMessage(MenuMode.Analysis);
            SwitchToInteractionMenu("Analysis Mode");
        }

        public void Cancel()
        {
            client.SendMenuChangedMessage(MenuMode.None);
            SwitchToMainMenu();
        }

        private void SwitchToInteractionMenu(string header)
        {
            mainMenu.SetActive(false);
            interactionMenu.SetActive(true);
            networkConfigMenu.SetActive(false);
            headerText.text = header;
        }
    }
}
