using Interaction;
using Unity.Netcode;

namespace Networking
{
    public class NetworkingClient : NetworkBehaviour
    {
        private Menu _ui;
        
        public override void OnNetworkSpawn()
        {
            if (IsHost)
            {
                Destroy(this);
                return;
            }

            _ui = FindObjectOfType<Menu>();
        }
    }
}