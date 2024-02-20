using System;
using UnityEngine;

namespace Networking.openIAExtension
{
    public class OpenIaWebSocketClient : MonoBehaviour
    {
        [SerializeField]
        private bool https = false;
        
        [SerializeField]
        private string ip = "127.0.0.1";

        [SerializeField]
        private short port = 80;

        [SerializeField]
        private string path = "/";

        private WebSocketClient _ws;

        private ICommandInterpreter _interpreter;

        private async void Start()
        {
            _ws = new WebSocketClient($"{(https ? "wss" : "ws")}://{ip}:{port}{(path.StartsWith("/") ? path : "/" + path)}");
            _ws.OnText += HandleText;
            var negotiator = new OpenIaProtocolNegotiator();
            _ws.OnBinary += negotiator.Interpret;
            
            Debug.Log("Starting WebSocket client");
            await _ws.ConnectAsync();
            Debug.Log("Connected WebSocket client");
            var runTask = _ws.Run();

            // this section up to line 47 is in danger of a race condition, because the server could already
            // start sending packets and we will drop them while changing the listener
            // TODO
            var version = await negotiator.Negotiate(_ws);
            if (version == 0)
            {
                // no supported version matches
                await _ws.Close();
                _ws.Dispose();
                return;
            }

            _ws.OnBinary += HandleBinaryData;
            _ws.OnBinary -= negotiator.Interpret;
            
            await runTask;
            Debug.Log("WebSocket client stopped");
        }
        
        private void HandleText(string text)
        {
            Debug.Log($"WS text received: \"{text}\"");
        }
        
        private void HandleBinaryData(byte[] data)
        {
            Debug.Log($"WS bytes received: {BitConverter.ToString(data)}");
            _interpreter.Interpret(data);
        }
    }
}