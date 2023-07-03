using System;
using Interaction;
using JetBrains.Annotations;
using UnityEngine;

namespace Networking
{
    public class Client : MonoBehaviour
    {
        [SerializeField]
        private Menu menu;
        [SerializeField]
        private PlayerEventEmitter playerEventEmitter;

        [CanBeNull] private Player _player;
        public event Action<MenuMode> MenuModeChanged;
        public event Action<string> TextReceived;

        private void OnEnable()
        {
            playerEventEmitter.PlayerConnected += OnPlayerConnected;
        }

        private void OnDisable()
        {
            playerEventEmitter.PlayerConnected -= OnPlayerConnected;
            if (_player is null)
            {
                return;
            }
            _player.ClientMenuModeChanged -= HandleMenuChange;
            _player.ClientTextReceived -= HandleText;
        }

        public void SendMenuChangedMessage(MenuMode mode)
        {
            if (_player is null)
            {
                return;
            }
            _player.MenuModeServerRpc(mode);
            _player.TextServerRpc(mode.ToString());
        }

        public void SendSwipeMessage(bool inward, float endPointX, float endPointY, float angle)
        {
            if (_player is not null)
            {
                _player.SwipeServerRpc(inward, endPointX, endPointY, angle);
                _player.TextServerRpc("Cancel initiated from client");
            }
            menu.Cancel();
        }

        public void SendScaleMessage(float scale)
        {
            if (_player != null) _player.ScaleServerRpc(scale);
        }

        public void SendRotateMessage(float rotation)
        {
            if (_player != null) _player.RotateServerRpc(rotation);
        }
        
        public void SendRotateFullMessage(Quaternion rotation)
        {
            if (_player != null) _player.RotateAllServerRpc(rotation);
        }

        public void SendTransformMessage(Vector3 offset)
        {
            if (_player != null) _player.TransformServerRpc(offset);
        }

        public void SendTiltMessage(bool isLeft)
        {
            if (_player != null) _player.TiltServerRpc(isLeft);
        }

        public void SendShakeMessage(int count)
        {
            if (_player != null) _player.ShakeServerRpc(count);
        }

        public void SendTapMessage(TapType type, float x, float y)
        {
            if (_player != null) _player.TapServerRpc(type, x, y);
            switch (type)
            {
                case TapType.HoldStart:
                    if (_player != null) _player.TextServerRpc("Hold Start initiated from client");
                    menu.StartMapping();
                    break;
                case TapType.HoldEnd:
                    if (_player != null) _player.TextServerRpc("Hold End initiated from client");
                    menu.StopMapping();
                    break;
                case TapType.Single:
                    if (_player != null) _player.TextServerRpc("Single Tap from client");
                    break;
                case TapType.Double:
                    if (_player != null) _player.TextServerRpc("Double Tap from client");
                    break;
                default:
                    Debug.Log($"{nameof(SendTapMessage)} received unknown tap type: {type}");
                    break;
            }
        }

        public void SendTextMessage(string text)
        {
            if (_player != null) _player.TextServerRpc(text);
        }

        private void OnPlayerConnected(Player p)
        {
            _player = p;
            
            if (_player == null)
            {
                return;
            }
            
            _player.ClientMenuModeChanged += HandleMenuChange;
            _player.ClientTextReceived += HandleText;
        }
        
        private void HandleMenuChange(MenuMode mode) => MenuModeChanged?.Invoke(mode);

        private void HandleText(string text) => TextReceived?.Invoke(text);
    }
}
