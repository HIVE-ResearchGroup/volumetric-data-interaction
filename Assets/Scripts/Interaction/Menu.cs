using Networking;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// Menu class
    /// Toggle between menu and detail view
    /// </summary>
    public class Menu : MonoBehaviour
    {
        [SerializeField]
        private NetworkingCommunicator comm;
        [SerializeField]
        private GameObject mainMenu;
        [SerializeField]
        private GameObject interactionMenu;
        [SerializeField]
        private GameObject networkConfigMenu;

        public void StartSelection()
        {
            Debug.Log("Selection");
            comm.MenuModeServerRpc(MenuMode.Selection);
            comm.TextServerRpc("Selection");
            SwitchToInteractionMenu();
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
            SwitchToInteractionMenu();
        }

        public void StartNetConfig()
        {
            Debug.Log("Network Config");
            SwitchToNetworkConfigMenu();
        }

        public void StopNetConfig()
        {
            Debug.Log("Stopped Network Config");
            SwitchToMainMenu();
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
            SwitchToInteractionMenu();
        }

        private void SwitchToMainMenu()
        {
            mainMenu.SetActive(true);
            interactionMenu.SetActive(false);
            networkConfigMenu.SetActive(false);
        }

        private void SwitchToInteractionMenu()
        {
            mainMenu.SetActive(false);
            interactionMenu.SetActive(true);
            networkConfigMenu.SetActive(false);
        }

        private void SwitchToNetworkConfigMenu()
        {
            mainMenu.SetActive(false);
            interactionMenu.SetActive(false);
            networkConfigMenu.SetActive(true);
        }
    }
}
