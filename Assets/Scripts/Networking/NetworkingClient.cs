using Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class NetworkingClient : NetworkBehaviour
    {
        [SerializeField] private Menu _ui;
        
        public override void OnNetworkSpawn()
        {
            if (!IsClient)
            {
                Destroy(gameObject);
                return;
            }

            _ui = FindObjectOfType<Menu>();
        }
    }
}