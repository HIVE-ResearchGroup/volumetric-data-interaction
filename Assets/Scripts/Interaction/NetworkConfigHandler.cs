using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Interaction
{
    public class NetworkConfigHandler : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField ipText;

        [SerializeField]
        private NetworkManager netMan;

        private UnityTransport _unityTransport;

        private void Awake()
        {
            _unityTransport = netMan.GetComponent<UnityTransport>();
        }

        public void TryReconnect()
        {
            var ip = ipText.text;
            Debug.Log($"IP entered: {ip}");
            _unityTransport.ConnectionData.Address = ip;
            netMan.StartClient();
        }
    }
}
