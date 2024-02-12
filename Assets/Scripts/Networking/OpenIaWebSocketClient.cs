using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
    public class OpenIaWebSocketClient : IDisposable
    {
        private readonly string _url;
        private readonly SimpleWebSocketClient _ws;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly OpenIACommandInterpreter _interpreter;

        public OpenIaWebSocketClient(string url, OpenIACommandInterpreter interpreter)
        {
            _url = url;
            _ws = new SimpleWebSocketClient();
            _cancellationTokenSource = new CancellationTokenSource();
            _interpreter = interpreter;
            _ws.OnText += HandleText;
            _ws.OnBinary += HandleBinaryData;
        }

        public async Task ConnectAsync() => await _ws.ConnectAsync(_url, _cancellationTokenSource.Token);

        public async Task Run()
        {
            await _ws.SendAsync("hello", _cancellationTokenSource.Token);
            await _ws.Run(_cancellationTokenSource.Token);
            _cancellationTokenSource.Dispose();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _ws?.Dispose();
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
