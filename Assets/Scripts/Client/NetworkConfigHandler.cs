using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Client
{
    public class NetworkConfigHandler : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField ipText;
        [SerializeField]
        private TMP_Text log;
        [SerializeField]
        private NetworkManager netMan;
        [SerializeField]
        private UnityTransport unityTransport;

        public void Connect()
        {
            var ip = ipText.text;
            Debug.Log($"IP entered: {ip}");
            unityTransport.ConnectionData.Address = ip;
            log.text += $"Connecting to {ip}\n";
            netMan.StartClient();
        }
    }
}
