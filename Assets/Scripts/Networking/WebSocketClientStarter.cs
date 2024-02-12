using System;
using UnityEngine;

namespace Networking
{
    public class WebSocketClientStarter : MonoBehaviour
    {
        [SerializeField]
        private bool https = false;
        
        [SerializeField]
        private string ip = "127.0.0.1";

        [SerializeField]
        private short port = 80;

        [SerializeField]
        private string path = "/";

        private OpenIaWebSocketClient _ws;

        private void Awake()
        {
            var interpreter = new OpenIACommandInterpreter();
            // TODO setup callbacks
            
            _ws = new OpenIaWebSocketClient($"{(https ? "wss" : "ws")}://{ip}:{port}{(path.StartsWith("/") ? path : "/" + path)}", interpreter);
        }

        private async void Start()
        {
            Debug.Log("Starting WebSocket client");
            await _ws.ConnectAsync();
            Debug.Log("Connected WebSocket client");
            await _ws.Run();
            Debug.Log("WebSocket client stopped");
        }
    }
}