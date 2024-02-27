using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking.screenExtension
{
    public class ScreenServer : MonoBehaviour
    {
        [SerializeField]
        private int port = 8642;
        
        private TcpListener _server;

        private readonly Dictionary<int, TcpClient> _clients = new();

        private void Awake()
        {
            _server = new TcpListener(IPAddress.Loopback, port);
        }

        private async void Start()
        {
            _server.Start();
            Debug.Log($"Screen server started on port {port}.");

            while (true)
            {
                var client = await _server.AcceptTcpClientAsync();
                await using var stream = client.GetStream();
                var buffer = new Memory<byte>();
                var bytes = await stream.ReadAsync(buffer);
                var id = BitConverter.ToInt32(buffer.Span);
                _clients.Add(id, client);
            }
        }

        public async Task Send(int id, Texture2D data)
        {
            var colors = data.GetPixels32();
            var bytes = new byte[colors.Length * 4];

            for (var i = 0; i < colors.Length; i++)
            {
                bytes[i * 4] = colors[i].r;
                bytes[i * 4 + 1] = colors[i].g;
                bytes[i * 4 + 2] = colors[i].b;
                bytes[i * 4 + 3] = colors[i].a;
            }
            
            await using var stream = _clients[id].GetStream();
            await stream.WriteAsync(bytes);
        }
    }
}