using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class HostStarter : MonoBehaviour
    {
        [SerializeField] private NetworkManager manager;
        
        private void Awake()
        {
            manager.StartHost();
        }
    }
}
