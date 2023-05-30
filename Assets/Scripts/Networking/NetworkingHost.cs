using Unity.Netcode;

namespace Networking
{
    public class NetworkingHost : NetworkBehaviour
    {
        private HostReferencesManager _refMan;
        
        public override void OnNetworkSpawn()
        {
            if (!IsHost)
            {
                Destroy(this);
                return;
            }

            _refMan = FindObjectOfType<HostReferencesManager>();
            _refMan.ray.SetActive(false);
        }
    }
}