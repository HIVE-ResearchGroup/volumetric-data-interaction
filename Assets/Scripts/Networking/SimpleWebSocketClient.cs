using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
    public class SimpleWebSocketClient : IDisposable
    {
        public const int BUFFER_SIZE = 8192;
        
        private ClientWebSocket _cws;
        private Task _recLoop;
        
        public event Action<string> OnText;
        public event Action<byte[]> OnBinary;

        public SimpleWebSocketClient()
        {
            _cws = new ClientWebSocket();
        }
        
        public async Task ConnectAsync(string url)
        {
            await _cws.ConnectAsync(new Uri(url), CancellationToken.None);
            _recLoop = Task.Run(ReceiveLoop);
        }

        public Task SendAsync(string text) => _cws.SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, CancellationToken.None);

        public Task SendAsync(byte[] data) => _cws.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);

        public Task Close() => _cws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        
        public void Dispose() => _cws?.Dispose();

        private async Task ReceiveLoop()
        {
            var buffer = new ArraySegment<byte>(new byte[BUFFER_SIZE]);

            while (_cws.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();

                WebSocketReceiveResult result;
                do
                {
                    result = await _cws.ReceiveAsync(buffer, CancellationToken.None);
                    await ms.WriteAsync(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            var msg = await reader.ReadToEndAsync();
                            Debug.Log($"WebSocket message received: {msg}");
                            OnText?.Invoke(msg);
                        }
                        break;
                    case WebSocketMessageType.Binary:
                        OnBinary?.Invoke(ms.GetBuffer());
                        break;
                    case WebSocketMessageType.Close:
                        Debug.Log("WebSocket Close requested");
                        await Close();
                        break;
                    default:
                        Debug.LogError($"WebSocket MessageType not recognized: {result.MessageType}");
                        break;
                }
            }
        }
    }
}