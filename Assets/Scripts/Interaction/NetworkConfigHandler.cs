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

        [SerializeField]
        private NetworkManager netMan;

        public void TryReconnect()
        {
            var ip = ipText.text;
            netMan.GetComponent<UnityTransport>().ConnectionData.Address = ip;
            netMan.StartClient();
        }
    }
}
