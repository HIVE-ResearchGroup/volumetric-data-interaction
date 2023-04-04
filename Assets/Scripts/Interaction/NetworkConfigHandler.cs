using Assets.Scripts.Network;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Interaction
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
