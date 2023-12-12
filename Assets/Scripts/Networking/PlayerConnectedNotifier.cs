using System;

namespace Networking
{
    public static class PlayerConnectedNotifier
    {
        public static event Action<Player> OnPlayerConnected;

        public static void Register(Player player) => OnPlayerConnected?.Invoke(player);
    }
}
