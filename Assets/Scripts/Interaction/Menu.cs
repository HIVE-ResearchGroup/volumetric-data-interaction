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
        private NetworkingCommunicator comm;
        [SerializeField]
        private GameObject mainMenu;
        [SerializeField]
        private GameObject interactionMenu;
        [SerializeField]
        private GameObject networkConfigMenu;

        public Text modeTitle;

        public MenuMode mode;
       
        private void Start()
        {
            mode = MenuMode.None;
        }

        public void StartSelection()
        {
            Debug.Log("Selection");
            mode = MenuMode.Selection;
            comm.MenuModeServerRpc(mode);

            mainMenu.SetActive(false);
            interactionMenu.SetActive(true);
            modeTitle.text = "Selection Mode";
        }

        public void SelectedObject()
        {
            // set object as gameobject in a specific script?
            mode = MenuMode.Selected;
            comm.MenuModeServerRpc(mode);
            comm.TextServerRpc("Selected");
            modeTitle.text = "Selected Mode";
        }

        public void StartMapping()
        {
            mode = MenuMode.Mapping;
            comm.MenuModeServerRpc(mode);
            comm.TextServerRpc("Start mapping");
            modeTitle.text = "Mapping Mode";
        }

        public void StopMapping()
        {
            mode = MenuMode.Selected;
            comm.MenuModeServerRpc(mode);
            comm.TextServerRpc("Stop mapping");
            modeTitle.text = "Selected Mode";
        }

        public void StartAnalysis()
        {
            mode = MenuMode.Analysis;
            comm.MenuModeServerRpc(mode);
            mainMenu.SetActive(false);
            interactionMenu.SetActive(true);
            modeTitle.text = "Analysis Mode";
        }

        public void StartNetConfig()
        {
            Debug.Log("Network Config");
            mainMenu.SetActive(false);
            interactionMenu.SetActive(false);
            networkConfigMenu.SetActive(true);
        }

        public void StopNetConfig()
        {
            Debug.Log("Stopped Network Config");
            networkConfigMenu.SetActive(false);
            mainMenu.SetActive(true);
        }

        public void Cancel()
        {
            mode = MenuMode.None;
            comm.MenuModeServerRpc(mode);
            mainMenu.SetActive(true);
            interactionMenu.SetActive(false);
        }

        public void SendDebug(string text)
        {
            comm.TextServerRpc(text);

            mode = MenuMode.Mapping;
            mainMenu.SetActive(false);
            interactionMenu.SetActive(true);
            modeTitle.text = text;
        }
    }
}
