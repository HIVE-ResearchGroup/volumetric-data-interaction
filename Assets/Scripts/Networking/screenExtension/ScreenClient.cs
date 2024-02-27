using System;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

namespace Networking.screenExtension
{
    public class ScreenClient : MonoBehaviour
    {
        [SerializeField]
        private string ip = "127.0.0.1";

        [SerializeField]
        private short port = 8642;

        [SerializeField]
        private int id = 1;

        [SerializeField]
        private Image image;

        private async void Start()
        {
            using var client = new TcpClient();
            await client.ConnectAsync(ip, port);
            await using var stream = client.GetStream();

            var bytes = BitConverter.GetBytes(id);
            await stream.WriteAsync(bytes, 0, bytes.Length);

            var buffer = new Memory<byte>();
            while (await stream.ReadAsync(buffer) > 0)
            {
                var tex = new Texture2D(2, 2);
                var colors = new Color32[buffer.Length / 4];
                var bufferArray = buffer.Span.ToArray();
                for (var i = 0; i < colors.Length; i++)
                {
                    colors[i].r = bufferArray[i * 4];
                    colors[i].g = bufferArray[i * 4 + 1];
                    colors[i].b = bufferArray[i * 4 + 2];
                    colors[i].a = bufferArray[i * 4 + 3];
                }
                tex.SetPixels32(colors);
                image.material.mainTexture = tex;
            }
            
            Debug.LogWarning("Client loop has stopped!");
        }
    }
}