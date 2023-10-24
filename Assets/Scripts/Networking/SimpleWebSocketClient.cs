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
        private const int BUFFER_SIZE = 8192;
        
        private readonly ClientWebSocket _cws = new();
        
        public event Action<string> OnText;
        public event Action<byte[]> OnBinary;

        public async Task ConnectAsync(string url, CancellationToken cancellationToken = default)
        {
            await _cws.ConnectAsync(new Uri(url), cancellationToken);
        }

        public Task SendAsync(string text, CancellationToken cancellationToken = default) => _cws.SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, cancellationToken);

        public Task SendAsync(byte[] data, CancellationToken cancellationToken = default) => _cws.SendAsync(data, WebSocketMessageType.Binary, true, cancellationToken);

        public Task Close(CancellationToken cancellationToken = default) => _cws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
        
        public void Dispose() => _cws?.Dispose();

        public async Task Run(CancellationToken cancellationToken)
        {
            var buffer = new ArraySegment<byte>(new byte[BUFFER_SIZE]);

            while (_cws.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();

                WebSocketReceiveResult result;
                do
                {
                    result = await _cws.ReceiveAsync(buffer, cancellationToken);
                    await ms.WriteAsync(buffer.Array, buffer.Offset, result.Count, cancellationToken);
                } while (!result.EndOfMessage);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
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
                        Debug.Log("WebSocket binary message received");
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