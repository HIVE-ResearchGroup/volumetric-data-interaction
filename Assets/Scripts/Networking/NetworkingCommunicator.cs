using System;
using Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class NetworkingCommunicator : NetworkBehaviour
    {
        public event Action<MenuMode> ModeChanged;
        public event Action<int> ShakeCompleted;
        public event Action<bool> Tilted;
        public event Action<TapType, float, float> Tapped;
        public event Action<bool, float, float, float> Swiped;
        public event Action<float> Scaled;
        public event Action<float> Rotated;
        public event Action<Vector3> RotatedFull;
        public event Action<string> TextReceived;

        public event Action<MenuMode> ClientMenuModeChanged;
        public event Action<string> ClientTextReceived;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            var proxy = FindObjectOfType<NetworkingCommunicatorProxy>();
            if (proxy is null)
            {
                Debug.LogWarning($"{nameof(NetworkingCommunicatorProxy)} not found! Nothing registered! RPC calls will not work!");
                return;
            }
            proxy.Register(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void MenuModeServerRpc(MenuMode mode) => ModeChanged?.Invoke(mode);

        [ServerRpc(RequireOwnership = false)]
        public void ShakeServerRpc(int count) => ShakeCompleted?.Invoke(count);

        [ServerRpc(RequireOwnership = false)]
        public void TiltServerRpc(bool isLeft) => Tilted?.Invoke(isLeft);

        [ServerRpc(RequireOwnership = false)]
        public void TapServerRpc(TapType type, float x, float y) => Tapped?.Invoke(type, x, y);

        [ServerRpc(RequireOwnership = false)]
        public void SwipeServerRpc(bool inward, float endPointX, float endPointY, float angle) => Swiped?.Invoke(inward, endPointX, endPointY, angle);

        [ServerRpc(RequireOwnership = false)]
        public void ScaleServerRpc(float scale) => Scaled?.Invoke(scale);

        [ServerRpc(RequireOwnership = false)]
        public void RotateServerRpc(float rotate) => Rotated?.Invoke(rotate);

        [ServerRpc(RequireOwnership = false)]
        public void RotateFullServerRpc(Vector3 rotation) => RotatedFull?.Invoke(rotation);
        
        [ServerRpc(RequireOwnership = false)]
        public void TextServerRpc(string text) => TextReceived?.Invoke(text);

        [ClientRpc]
        public void MenuModeClientRpc(MenuMode mode) => ClientMenuModeChanged?.Invoke(mode);

        [ClientRpc]
        public void TextClientRpc(string text) => ClientTextReceived?.Invoke(text);
    }
}