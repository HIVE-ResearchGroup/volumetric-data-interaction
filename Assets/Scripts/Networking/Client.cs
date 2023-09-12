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
                _player.MenuModeServerRpc(mode);
            }
        }

        public void SendSwipeMessage(bool inward, float endPointX, float endPointY, float angle)
        {
            if (_player != null)
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
            if (_player != null)
            {
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
        }
        
        private void HandleMenuChange(MenuMode mode) => MenuModeChanged?.Invoke(mode);
    }
}
