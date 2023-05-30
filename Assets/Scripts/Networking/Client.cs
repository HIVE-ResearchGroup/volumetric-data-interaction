using System;
using Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class Client : NetworkBehaviour
    {
        [SerializeField] private Menu menu;

        private NetworkingCommunicator _comm;

        private void Start()
        {
            _comm = NetworkingCommunicator.Singleton;
            _comm.ClientMenuModeChanged += HandleMenuChange;
            _comm.ClientTextReceived += HandleText;
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
            _comm.TextServerRpc("Cancel initiated from client");
            menu.Cancel();
        }

        public void HandleTabMessage(TabType type)
        {
            switch (type)
            {
                case TabType.HoldStart:
                    _comm.TextServerRpc("Hold Start initiated from client");
                    menu.StartMapping();
                    break;
                case TabType.HoldEnd:
                    _comm.TextServerRpc("Hold End initiated from client");
                    menu.StopMapping();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
