using TMPro;
using UnityEngine;
using NetworkOld;

namespace Interaction
{
    public class NetworkConfigHandler : MonoBehaviour
    {
        [SerializeField]
        private Client client;
        [SerializeField]
        private TMP_Text ipText;

        public void TryReconnect()
        {
            string ip = ipText.text;
            client.SetIPAndReconnect(ip);
        }
    }
}
