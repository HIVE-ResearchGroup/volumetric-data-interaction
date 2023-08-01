using UnityEngine;

namespace Networking
{
    public class OpenIaWebSocketClient : MonoBehaviour
    {
        [SerializeField]
        private string url = "";
        
        private SimpleWebSocketClient _ws;

        private void Start()
        {
            _ws = new SimpleWebSocketClient();
            _ws.ConnectAsync(url).Wait();
        }

        private void OnDestroy()
        {
            _ws.Dispose();
        }
    }
}
