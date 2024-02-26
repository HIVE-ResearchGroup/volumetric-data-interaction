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
            _ws.OnBinary += HandleBinaryData;
            
            var negotiator = new OpenIaProtocolNegotiator(_ws);
            _interpreter = negotiator;
            
            Debug.Log("Starting WebSocket client");
            await _ws.ConnectAsync();
            Debug.Log("Connected WebSocket client");
            var runTask = _ws.Run();

            try
            {
                _interpreter = await negotiator.Negotiate();
            }
            catch (NoProtocolMatchException e)
            {
                // no supported version matches
                await _ws.Close();
                _ws.Dispose();
                return;
            }
            
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