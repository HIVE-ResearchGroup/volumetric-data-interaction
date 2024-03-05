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

        private readonly Dictionary<int, (TcpClient, NetworkStream)> _clients = new();

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
                var stream = client.GetStream();
                var buffer = new byte[4];
                var bytes = await stream.ReadAsync(buffer, 0, 4);
                var id = BitConverter.ToInt32(buffer);
                _clients.Add(id, (client, stream));
                Debug.Log($"Client {id} connected");
            }
        }

        public async Task Send(int id, Texture2D data)
        {
            var colors = data.GetPixels32();
            var dimBuffer = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(data.width), 0, dimBuffer, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(data.height), 0, dimBuffer, 4, 4);
            
            var bytes = new byte[colors.Length * 4];

            for (var i = 0; i < colors.Length; i++)
            {
                bytes[i * 4] = colors[i].r;
                bytes[i * 4 + 1] = colors[i].g;
                bytes[i * 4 + 2] = colors[i].b;
                bytes[i * 4 + 3] = colors[i].a;
            }

            var (_, stream) = _clients[id];
            await stream.WriteAsync(dimBuffer);
            await stream.WriteAsync(bytes);
        }
    }
}