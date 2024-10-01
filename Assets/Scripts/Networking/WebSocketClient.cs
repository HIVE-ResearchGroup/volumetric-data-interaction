#nullable enable

using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
    public class WebSocketClient : IDisposable
    {
        private const int BufferSize = 1024;
        
        private readonly ClientWebSocket _cws = new();
        private readonly string _url;
        private readonly Func<byte[], Task> _binaryCallback;
        private readonly Func<string, Task> _textCallback;

        private readonly CancellationTokenSource _cancellationTokenSource;
        
        public WebSocketClient(string url, Func<byte[], Task> binaryCallback, Func<string, Task> textCallback)
        {
            _url = url;
            _binaryCallback = binaryCallback;
            _textCallback = textCallback;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync() => await _cws.ConnectAsync(new Uri(_url), _cancellationTokenSource.Token);

        public async Task SendAsync(string text) => await _cws.SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);

        public async Task SendAsync(byte[] data) => await _cws.SendAsync(data, WebSocketMessageType.Binary, true, _cancellationTokenSource.Token);

        public async Task Run()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            
            var buffer = new ArraySegment<byte>(new byte[BufferSize]);

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
                            await _textCallback.Invoke(msg);
                        }
                        break;
                    case WebSocketMessageType.Binary:
                        Debug.Log("WebSocket binary message received");
                        await _binaryCallback.Invoke(ms.ToArray());
                        break;
                    case WebSocketMessageType.Close:
                        Debug.Log("WebSocket Close requested");
                        await _cws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close Answered", cancellationToken);
                        break;
                    default:
                        Debug.LogError($"WebSocket MessageType not recognized: {result.MessageType}");
                        break;
                }
            }
            _cancellationTokenSource.Dispose();
        }

        public async Task Close() => await _cws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close Requested", _cancellationTokenSource.Token);

        public void Dispose()
        {
            // https://stackoverflow.com/a/51220106
            // CancellationTokenSource should only Dispose when everything is finished. (As seen above in Run())
            // Cancel() does the same cleanup, but also cancels the token without race condition
            _cancellationTokenSource.Cancel();
            _cws.Dispose();
        }
    }
}
