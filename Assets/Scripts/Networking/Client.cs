using System;
using Interaction;
using UnityEngine;

namespace Networking
{
    public class Client : MonoBehaviour
    {
        [SerializeField]
        private Menu menu;
        [SerializeField]
        private NetworkingCommunicatorProxy comm;

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

        private void HandleText(string text) => menu.SendDebug(text);

        public void HandleSwipeMessage(bool inward, float endPointX, float endPointY, float angle)
        {
            comm.SwipeServerRpc(inward, endPointX, endPointY, angle);
            comm.TextServerRpc("Cancel initiated from client");
            menu.Cancel();
        }

        public void HandleScaleMessage(float scale) => comm.ScaleServerRpc(scale);

        public void HandleRotateMessage(float rotation) => comm.RotateServerRpc(rotation);
        public void HandleTiltMessage(bool isLeft) => comm.TiltServerRpc(isLeft);
        public void HandleShakeMessage(int count) => comm.ShakeServerRpc(count); 

        public void HandleTapMessage(TapType type)
        {
            comm.TapServerRpc(type);
            switch (type)
            {
                case TapType.HoldStart:
                    comm.TextServerRpc("Hold Start initiated from client");
                    menu.StartMapping();
                    break;
                case TapType.HoldEnd:
                    comm.TextServerRpc("Hold End initiated from client");
                    menu.StopMapping();
                    break;
                case TapType.Single:
                    comm.TextServerRpc("Single Tap from client");
                    break;
                case TapType.Double:
                    comm.TextServerRpc("Double Tap from client");
                    break;
            }
        }
    }
}
