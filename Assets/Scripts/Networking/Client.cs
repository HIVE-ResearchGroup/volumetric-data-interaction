using System;
using Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class Client : NetworkBehaviour
    {
        [SerializeField] private Menu menu;

        [SerializeField] private NetworkingCommunicator comm;

        private void OnEnable()
        {
            comm.ClientMenuModeChanged += HandleMenuChange;
            comm.ClientTextReceived += HandleText;
        }

        private void OnDisable()
        {
            comm.ClientMenuModeChanged -= HandleMenuChange;
            comm.ClientTextReceived -= HandleText;
        }


        private void HandleMenuChange(MenuMode mode)
        {
            switch (mode)
            {
                case MenuMode.Selected:
                    menu.SelectedObject();
                    break;
                case MenuMode.None:
                    menu.Cancel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private void HandleText(string text)
        {
            menu.SendDebug(text);
        }

        public void HandleSwipeMessage()
        {
            comm.TextServerRpc("Cancel initiated from client");
            menu.Cancel();
        }

        public void HandleTabMessage(TabType type)
        {
            switch (type)
            {
                case TabType.HoldStart:
                    comm.TextServerRpc("Hold Start initiated from client");
                    menu.StartMapping();
                    break;
                case TabType.HoldEnd:
                    comm.TextServerRpc("Hold End initiated from client");
                    menu.StopMapping();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
