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
        private NetworkingCommunicator comm;

        public event Action<MenuMode> MenuModeChanged;
        public event Action<string> TextReceived;

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


        private void HandleMenuChange(MenuMode mode) => MenuModeChanged?.Invoke(mode);

        private void HandleText(string text) => TextReceived?.Invoke(text);

        public void SendMenuChangedMessage(MenuMode mode)
        {
            comm.MenuModeServerRpc(mode);
            comm.TextServerRpc(mode.ToString());
        }

        public void SendSwipeMessage(bool inward, float endPointX, float endPointY, float angle)
        {
            comm.SwipeServerRpc(inward, endPointX, endPointY, angle);
            comm.TextServerRpc("Cancel initiated from client");
            menu.Cancel();
        }

        public void SendScaleMessage(float scale) => comm.ScaleServerRpc(scale);

        public void SendRotateMessage(float rotation) => comm.RotateServerRpc(rotation);
        
        public void SendRotateFullMessage(Quaternion rotation) => comm.RotateAllServerRpc(rotation);
        
        public void SendTransformMessage(Vector3 offset) => comm.TransformServerRpc(offset);
        
        public void SendTiltMessage(bool isLeft) => comm.TiltServerRpc(isLeft);
        
        public void SendShakeMessage(int count) => comm.ShakeServerRpc(count); 

        public void SendTapMessage(TapType type, float x, float y)
        {
            comm.TapServerRpc(type, x, y);
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
                default:
                    Debug.Log($"{nameof(SendTapMessage)} received unknown tap type: {type}");
                    break;
            }
        }

        public void SendTextMessage(string text) => comm.TextServerRpc(text);
    }
}
