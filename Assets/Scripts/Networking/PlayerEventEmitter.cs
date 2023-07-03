using System;
using UnityEngine;

namespace Networking
{
    public class PlayerEventEmitter : MonoBehaviour
    {
        public event Action<Player> PlayerConnected;
        
        public void Register(Player player) => PlayerConnected?.Invoke(player);
    }
}
