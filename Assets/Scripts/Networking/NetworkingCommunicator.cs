using System;
using Interaction;
using Unity.Netcode;

namespace Networking
{
    public class NetworkingCommunicator : NetworkBehaviour
    {
        private static NetworkingCommunicator _singleton;

        public event Action<MenuMode> ModeChanged;
        public event Action<int> ShakeCompleted;
        public event Action<bool> Tilted;
        public event Action<TapType> Tapped;
        public event Action<bool, float, float, float> Swiped;
        public event Action<float> Scaled;
        public event Action<float> Rotated; 
        public event Action<string> TextReceived;

        public event Action<MenuMode> ClientMenuModeChanged;
        public event Action<string> ClientTextReceived;

        private NetworkingCommunicator() {}

        private void Awake()
        {
            if (_singleton is not null && _singleton != this)
            {
                Destroy(this);
            }

            _singleton = this;
        }

        [ServerRpc]
        public void MenuModeServerRpc(MenuMode mode) => ModeChanged?.Invoke(mode);

        [ServerRpc]
        public void ShakeServerRpc(int count) => ShakeCompleted?.Invoke(count);

        [ServerRpc]
        public void TiltServerRpc(bool isLeft) => Tilted?.Invoke(isLeft);

        [ServerRpc]
        public void TapServerRpc(TapType type) => Tapped?.Invoke(type);

        [ServerRpc]
        public void SwipeServerRpc(bool inward, float endPointX, float endPointY, float angle) => Swiped?.Invoke(inward, endPointX, endPointY, angle);

        [ServerRpc]
        public void ScaleServerRpc(float scale) => Scaled?.Invoke(scale);

        [ServerRpc]
        public void RotateServerRpc(float rotate) => Rotated?.Invoke(rotate);
        
        [ServerRpc]
        public void TextServerRpc(string text) => TextReceived?.Invoke(text);

        [ClientRpc]
        public void MenuModeClientRpc(MenuMode mode) => ClientMenuModeChanged?.Invoke(mode);

        [ClientRpc]
        public void TextClientRpc(string text) => ClientTextReceived?.Invoke(text);
    }
}