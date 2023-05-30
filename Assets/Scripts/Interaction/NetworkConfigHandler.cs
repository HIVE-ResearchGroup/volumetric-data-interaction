using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Interaction
{
    public class NetworkConfigHandler : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text ipText;

        public void TryReconnect()
        {
            var ip = ipText.text;
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = ip;
            NetworkManager.Singleton.StartClient();
        }
    }
}
