using System;
using System.Threading.Tasks;
using Networking.openIAExtension.Commands;
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
        private ushort port = 80;

        [SerializeField]
        private string path = "/";

        private WebSocketClient _ws;

        private ICommandInterpreter _interpreter;

        private ICommandSender _sender;

        private async void Start()
        {
            
            _ws = new WebSocketClient($"{(https ? "wss" : "ws")}://{ip}:{port}{(path.StartsWith("/") ? path : "/" + path)}");
            _ws.OnText += HandleText;
            _ws.OnBinary += HandleBinaryData;
            
            var negotiator = new ProtocolNegotiator(_ws);
            _interpreter = negotiator;
            
            Debug.Log("Starting WebSocket client");
            await _ws.ConnectAsync();
            Debug.Log("Connected WebSocket client");
            var runTask = _ws.Run();

            try
            {
                (_interpreter, _sender) = await negotiator.Negotiate();
            }
            catch (NoProtocolMatchException)
            {
                // no supported version matches
                await _ws.Close();
                _ws.Dispose();
                return;
            }
            
            await runTask;
            Debug.Log("WebSocket client stopped");
        }

        public async Task Send(ICommand cmd) => await _sender.Send(cmd);
        
        private void HandleText(string text)
        {
            Debug.Log($"WS text received: \"{text}\"");
        }
        
        private async void HandleBinaryData(byte[] data)
        {
            Debug.Log($"WS bytes received: {BitConverter.ToString(data)}");
            await _interpreter.Interpret(data);
        }
    }
}