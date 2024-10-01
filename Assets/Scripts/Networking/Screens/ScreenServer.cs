#nullable enable

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Extensions;
using UnityEngine;

namespace Networking.Screens
{
    public class ScreenServer : MonoBehaviour
    {
        public static ScreenServer Instance { get; private set; } = null!;
        
        [SerializeField]
        private int port = Ports.ScreenPort;
        
        private TcpListener server = null!;

        private readonly List<Screen> screens = new();
        
        private readonly Dictionary<int, (TcpClient, NetworkStream)> clients = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                server = new TcpListener(IPAddress.Any, port);
                for (var i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).TryGetComponent<Screen>(out var screen))
                    {
                        screens.Add(screen);
                    }
                }
            }
            else
            {
                Destroy(this);
            }
        }

        private async void Start()
        {
            server.Start();
            Debug.Log($"Screen server started on port {port}.");

            while (true)
            {
                try
                {
                    var client = await server.AcceptTcpClientAsync();
                    var stream = client.GetStream();
                    var buffer = new byte[IDAdvertisement.Size];
                    await stream.ReadAllAsync(buffer, 0, buffer.Length);
                    var idAd = IDAdvertisement.FromByteArray(buffer);
                    clients.Add(idAd.ID, (client, stream));
                    Debug.Log($"Client {idAd.ID} connected");
                }
                catch
                {
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var (_, (c, _)) in clients)
            {
                c.Close();
            }
            server.Stop();
        }

        public async Task SendAsync(Vector3 trackerPosition, Vector3 trackerPointDirection, Texture2D data)
        {
            if (!FindScreen(out var screen, trackerPosition, trackerPointDirection))
            {
                return;
            }
            
            Debug.Log($"Sending to screen {screen}");

            var imageData = new ImageData(data.GetPixels32());
            var dims = new Dimensions(data.width, data.height);
            var (_, stream) = clients[screen];
            await stream.WriteAsync(dims.ToByteArray());
            await stream.WriteAsync(imageData.ToByteArray());
        }

        private bool FindScreen(out int id, Vector3 trackerPosition, Vector3 trackerPointDirection)
        {
            const float maxCheckDistance = 100.0f;

            foreach (var s in screens)
            {
                var ray = new Ray(trackerPosition, trackerPointDirection);
                if (s.BoxCollider.Raycast(ray, out _, maxCheckDistance))
                {
                    id = s.ID;
                    return true;
                }
            }

            id = 0;
            return false;
        }
    }
}