using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
    // TODO missing protocol definition, must be implemented later
    public class OpenIaWebSocketClient : MonoBehaviour
    {
        [SerializeField]
        private string url = "";
        
        private SimpleWebSocketClient _ws;

        private void Awake()
        {
            _ws = new SimpleWebSocketClient();
        }

        private void OnDestroy()
        {
            _ws.Dispose();
        }

        public Task ConnectAsync() => _ws.ConnectAsync(url);
    }
}
