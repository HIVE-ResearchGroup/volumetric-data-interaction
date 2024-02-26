using System.Net.Sockets;
using UnityEngine;

namespace Networking.screenExtension
{
    public class ScreenClient : MonoBehaviour
    {
        [SerializeField]
        private string ip = "127.0.0.1";

        [SerializeField]
        private short port = 8642;

        private async void Start()
        {
            using var client = new TcpClient();
            await client.ConnectAsync(ip, port);
            await using var stream = client.GetStream();

            var buffer = new byte[1024];
            while (await stream.ReadAsync(buffer) > 0)
            {
                // TODO form image
            }
        }
    }
}