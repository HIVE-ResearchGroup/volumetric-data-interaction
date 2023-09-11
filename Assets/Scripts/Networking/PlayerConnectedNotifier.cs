using System;
using UnityEngine;

namespace Networking
{
    public class PlayerConnectedNotifier : MonoBehaviour
    {
        public static PlayerConnectedNotifier Instance { get; private set; }
        
        public event Action<Player> OnPlayerConnected;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void Register(Player player) => OnPlayerConnected?.Invoke(player);
    }
}
