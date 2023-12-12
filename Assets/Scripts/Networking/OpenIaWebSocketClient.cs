using System;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
    // TODO missing protocol definition, must be implemented later
    public class OpenIaWebSocketClient : IDisposable
    {
        private readonly string _url;
        private readonly SimpleWebSocketClient _ws;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public OpenIaWebSocketClient(string url)
        {
            _url = url;
            _ws = new SimpleWebSocketClient();
            _cancellationTokenSource = new CancellationTokenSource();
            _ws.OnText += HandleText;
            _ws.OnBinary += HandleBinaryData;
        }

        public async Task ConnectAsync() => await _ws.ConnectAsync(_url, _cancellationTokenSource.Token);

        public async Task Run()
        {
            await _ws.Run(_cancellationTokenSource.Token)
                .ContinueWith(_ => _cancellationTokenSource.Dispose());
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _ws?.Dispose();
        }

        private void HandleText(string text)
        {
            
        }
        
        private void HandleBinaryData(byte[] data)
        {
            
        }
    }
}
