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
        // TODO switch with Client
        [SerializeField]
        private NetworkingCommunicator comm;
        [SerializeField]
        private GameObject mainMenu;
        [SerializeField]
        private GameObject interactionMenu;
        [SerializeField]
        private GameObject networkConfigMenu;
        [SerializeField]
        private Text headerText;

        public void SwitchToMainMenu()
        {
            mainMenu.SetActive(true);
            interactionMenu.SetActive(false);
            networkConfigMenu.SetActive(false);
        }
        
        public void StartSelection()
        {
            Debug.Log("Selection");
            comm.MenuModeServerRpc(MenuMode.Selection);
            comm.TextServerRpc("Selection");
            SwitchToInteractionMenu("Selection Mode");
        }

        public void SelectedObject()
        {
            // set object as gameobject in a specific script?
            comm.MenuModeServerRpc(MenuMode.Selected);
            comm.TextServerRpc("Selected");
        }

        public void StartMapping()
        {
            comm.MenuModeServerRpc(MenuMode.Mapping);
            comm.TextServerRpc("Start mapping");
        }

        public void StopMapping()
        {
            comm.TextServerRpc("Stop mapping");
            SelectedObject();
        }

        public void StartAnalysis()
        {
            comm.MenuModeServerRpc(MenuMode.Analysis);
            comm.TextServerRpc("Analysis");
            SwitchToInteractionMenu("Analysis Mode");
        }

        public void Cancel()
        {
            comm.MenuModeServerRpc(MenuMode.None);
            comm.TextServerRpc("Cancel");
            SwitchToMainMenu();
        }

        public void SendDebug(string text)
        {
            comm.TextServerRpc($"Debug: {text}");
            SwitchToInteractionMenu("Debug Mode");
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
