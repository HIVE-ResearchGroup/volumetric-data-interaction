using System;
using Client;
using JetBrains.Annotations;
using UnityEngine;

namespace Networking
{
    public class Client : MonoBehaviour
    {
        [SerializeField]
        private Menu menu;

        [CanBeNull] private Player _player;
        public event Action<MenuMode> MenuModeChanged;

        private void OnEnable()
        {
            PlayerConnectedNotifier.OnPlayerConnected += OnPlayerConnected;
        }

        private void OnDisable()
        {
            PlayerConnectedNotifier.OnPlayerConnected -= OnPlayerConnected;
            if (_player == null)
            {
                return;
            }
            _player.ClientMenuModeChanged -= HandleMenuChange;
        }

        public void SendMenuChangedMessage(MenuMode mode)
        {
            if (_player != null)
            {
                Debug.Log($"Sending menu change: {mode}");
                _player.MenuModeServerRpc(mode);
            }
        }

        public void SendSwipeMessage(bool inward, float endPointX, float endPointY, float angle)
        {
            if (_player != null)
            {
                Debug.Log("Sending swipe");
                _player.SwipeServerRpc(inward, endPointX, endPointY, angle);
                _player.TextServerRpc("Cancel initiated from client");
            }
            menu.Cancel();
        }

        public void SendScaleMessage(float scale)
        {
            if (_player != null)
            {
                Debug.Log($"Sending scale: {scale}");
                _player.ScaleServerRpc(scale);
            }
        }

        public void SendRotateMessage(float rotation)
        {
            if (_player != null)
            {
                Debug.Log($"Sending rotation: {rotation}");
                _player.RotateServerRpc(rotation);
            }
        }

        public void SendTiltMessage(bool isLeft)
        {
            if (_player != null)
            {
                Debug.Log($"Sending tilt {(isLeft ? "left" : "right")}");
                _player.TiltServerRpc(isLeft);
            }
        }

        public void SendShakeMessage(int count)
        {
            if (_player != null)
            {
                Debug.Log($"Sending shake: {count}");
                _player.ShakeServerRpc(count);
            }
        }

        public void SendTapMessage(TapType type, float x, float y)
        {
            if (_player != null)
            {
                Debug.Log($"Sending tap: {type} at ({x},{y})");
                _player.TapServerRpc(type, x, y);
            }

            switch (type)
            {
                case TapType.HoldStart:
                    menu.StartMapping();
                    break;
                case TapType.HoldEnd:
                    menu.StopMapping();
                    break;
            }
        }

        public void SendTextMessage(string text)
        {
            if (_player != null)
            {
                Debug.Log($"Sending a debug text message: {text}");
                _player.TextServerRpc(text);
            }
        }

        private void OnPlayerConnected(Player p)
        {
            if (p == null)
            {
                Debug.LogWarning("Connected player is null!");
                return;
            }
            if (!p.IsLocalPlayer)
            {
                Debug.Log("Connected player is not local player. Player will be ignored.");
                return;
            }
            Debug.Log("Local player connected");
            
            _player = p;
            _player.ClientMenuModeChanged += HandleMenuChange;
        }
        
        private void HandleMenuChange(MenuMode mode) => MenuModeChanged?.Invoke(mode);
    }
}
