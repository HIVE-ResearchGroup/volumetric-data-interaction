using System;
using Interaction;
using UnityEngine;

namespace Networking
{
    public class NetworkingCommunicatorProxy : MonoBehaviour
    {
        private NetworkingCommunicator _netComm;
    
        public event Action<MenuMode> ModeChanged;
        public event Action<int> ShakeCompleted;
        public event Action<bool> Tilted;
        public event Action<TapType, float, float> Tapped;
        public event Action<bool, float, float, float> Swiped;
        public event Action<float> Scaled;
        public event Action<float> Rotated;
        public event Action<Quaternion> RotatedAll;
        public event Action<Vector3> Transform;
        public event Action<string> TextReceived;

        public event Action<MenuMode> ClientMenuModeChanged;
        public event Action<string> ClientTextReceived;
    
        public void Register(NetworkingCommunicator netComm)
        {
            if (_netComm is not null)
            {
                OnDisable();
            }
            _netComm = netComm;
            _netComm.ModeChanged += ModeChanged;
            _netComm.ShakeCompleted += ShakeCompleted;
            _netComm.Tilted += Tilted;
            _netComm.Tapped += Tapped;
            _netComm.Swiped += Swiped;
            _netComm.Scaled += Scaled;
            _netComm.Rotated += Rotated;
            _netComm.RotatedAll += RotatedAll;
            _netComm.Transform += Transform;
            _netComm.TextReceived += TextReceived;
            _netComm.ClientMenuModeChanged += ClientMenuModeChanged;
            _netComm.ClientTextReceived += ClientTextReceived;
        }

        private void OnDisable()
        {
            if (_netComm is null)
            {
                return;
            }
        
            _netComm.ModeChanged -= ModeChanged;
            _netComm.ShakeCompleted -= ShakeCompleted;
            _netComm.Tilted -= Tilted;
            _netComm.Tapped -= Tapped;
            _netComm.Swiped -= Swiped;
            _netComm.Scaled -= Scaled;
            _netComm.Rotated -= Rotated;
            _netComm.RotatedAll -= RotatedAll;
            _netComm.Transform -= Transform;
            _netComm.TextReceived -= TextReceived;
            _netComm.ClientMenuModeChanged -= ClientMenuModeChanged;
            _netComm.ClientTextReceived -= ClientTextReceived;
        }

        public void MenuModeServerRpc(MenuMode mode) => _netComm.MenuModeServerRpc(mode);

        public void ShakeServerRpc(int count) => _netComm.ShakeServerRpc(count);

        public void TiltServerRpc(bool isLeft) => _netComm.TiltServerRpc(isLeft);

        public void TapServerRpc(TapType type, float x, float y) => _netComm.TapServerRpc(type, x, y);

        public void SwipeServerRpc(bool inward, float endPointX, float endPointY, float angle) =>
            _netComm.SwipeServerRpc(inward, endPointX, endPointY, angle);

        public void ScaleServerRpc(float scale) => _netComm.ScaleServerRpc(scale);

        public void RotateServerRpc(float rotate) => _netComm.RotateServerRpc(rotate);
        
        public void RotateAllServerRpc(Quaternion rotation) => _netComm.RotateAllServerRpc(rotation);

        public void TransformServerRpc(Vector3 offset) => _netComm.TransformServerRpc(offset);

        public void TextServerRpc(string text) => _netComm.TextServerRpc(text);

        public void MenuModeClientRpc(MenuMode mode) => _netComm.MenuModeClientRpc(mode);

        public void TextClientRpc(string text) => _netComm.TextClientRpc(text);
    }
}
