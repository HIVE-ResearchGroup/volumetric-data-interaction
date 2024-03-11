using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking.screenExtension
{
    public class ScreenServer : MonoBehaviour
    {
        private const float ConeAngle = 30.0f;
        
        [SerializeField]
        private int port = 8642;

        [SerializeField]
        private List<Screen> screens = new();

        private bool _running;
        
        private TcpListener _server;

        private readonly Dictionary<int, (TcpClient, NetworkStream)> _clients = new();

        private void Awake()
        {
            DontDestroyOnLoad(this);
            _server = new TcpListener(IPAddress.Loopback, port);
        }

        private async void Start()
        {
            _running = true;
            _server.Start();
            Debug.Log($"Screen server started on port {port}.");

            while (_running)
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

        private void OnDestroy()
        {
            _running = false;
            foreach (var (_, (c, _)) in _clients)
            {
                c.Close();
            }
            _server.Stop();
        }

        public async Task Send(Transform tracker, Texture2D data)
        {
            var screen = FindScreen(tracker);
            if (screen == -1)
            {
                return;
            }
            
            Debug.Log($"Sending to screen {screen}");
            
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

            var (_, stream) = _clients[screen];
            await stream.WriteAsync(dimBuffer);
            await stream.WriteAsync(bytes);
        }

        private int FindScreen(Transform tracker)
        {
            if (screens.Count == 0)
            {
                return -1;
            } 
            
            var tPos = tracker.position;
            var tRot = Vector3.Normalize(tracker.up);

            return screens
                .Select(s => (s.id, CalculateAngleToScreen(tPos, tRot, s.transform.position)))
                .Where(s => s.Item2 <= ConeAngle)
                .OrderBy(s => s.Item2)
                .DefaultIfEmpty((-1, 0.0f))
                .First()
                .Item1;
        }

        private static float CalculateAngleToScreen(Vector3 trackerPosition, Vector3 trackerRotation, Vector3 screenPosition)
        {
            // based on answer here: https://stackoverflow.com/questions/1167022/2d-geometry-how-to-check-if-a-point-is-inside-an-angle
            var vec = Vector3.Normalize(trackerPosition - screenPosition);
            var dot = Vector3.Dot(vec, trackerRotation);
            return Mathf.Acos(dot);
        }
    }
}