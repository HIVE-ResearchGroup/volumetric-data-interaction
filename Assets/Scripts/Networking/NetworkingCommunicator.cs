using System;
using Interaction;
using UnityEngine;

namespace Networking
{
    public class NetworkingCommunicator : MonoBehaviour
    {
        private Player _player;

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
    
        public void Register(Player player)
        {
            // only keep the newest registered player
            if (_player is not null)
            {
                OnDisable();
            }
            _player = player;
            _player.ModeChanged += ModeChanged;
            _player.ShakeCompleted += ShakeCompleted;
            _player.Tilted += Tilted;
            _player.Tapped += Tapped;
            _player.Swiped += Swiped;
            _player.Scaled += Scaled;
            _player.Rotated += Rotated;
            _player.RotatedAll += RotatedAll;
            _player.Transform += Transform;
            _player.TextReceived += TextReceived;
            _player.ClientMenuModeChanged += ClientMenuModeChanged;
            _player.ClientTextReceived += ClientTextReceived;
        }

        private void OnDisable()
        {
            if (_player is null)
            {
                return;
            }
        
            _player.ModeChanged -= ModeChanged;
            _player.ShakeCompleted -= ShakeCompleted;
            _player.Tilted -= Tilted;
            _player.Tapped -= Tapped;
            _player.Swiped -= Swiped;
            _player.Scaled -= Scaled;
            _player.Rotated -= Rotated;
            _player.RotatedAll -= RotatedAll;
            _player.Transform -= Transform;
            _player.TextReceived -= TextReceived;
            _player.ClientMenuModeChanged -= ClientMenuModeChanged;
            _player.ClientTextReceived -= ClientTextReceived;
        }

        public void MenuModeServerRpc(MenuMode mode) => _player.MenuModeServerRpc(mode);

        public void ShakeServerRpc(int count) => _player.ShakeServerRpc(count);

        public void TiltServerRpc(bool isLeft) => _player.TiltServerRpc(isLeft);

        public void TapServerRpc(TapType type, float x, float y) => _player.TapServerRpc(type, x, y);

        public void SwipeServerRpc(bool inward, float endPointX, float endPointY, float angle) =>
            _player.SwipeServerRpc(inward, endPointX, endPointY, angle);

        public void ScaleServerRpc(float scale) => _player.ScaleServerRpc(scale);

        public void RotateServerRpc(float rotate) => _player.RotateServerRpc(rotate);
        
        public void RotateAllServerRpc(Quaternion rotation) => _player.RotateAllServerRpc(rotation);

        public void TransformServerRpc(Vector3 offset) => _player.TransformServerRpc(offset);

        public void TextServerRpc(string text) => _player.TextServerRpc(text);

        public void MenuModeClientRpc(MenuMode mode) => _player.MenuModeClientRpc(mode);

        public void TextClientRpc(string text) => _player.TextClientRpc(text);
    }
}
