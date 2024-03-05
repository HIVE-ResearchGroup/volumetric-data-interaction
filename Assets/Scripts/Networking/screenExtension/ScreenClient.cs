using System;
using System.Collections.Generic;
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
        private RawImage image;

        private RectTransform _rect;

        private Vector2 _rectSize;
        
        private bool _running;

        private void Awake()
        {
            _rect = image.GetComponent<RectTransform>();
            _rectSize = _rect.sizeDelta;
        }

        private async void OnEnable()
        {
            _running = true;
            using var client = new TcpClient();
            await client.ConnectAsync(ip, port);
            await using var stream = client.GetStream();

            await stream.WriteAsync(BitConverter.GetBytes(id));
            Debug.Log($"ID sent {id}");

            var dimBuffer = new byte[8];
            
            while (_running)
            {
                var bytes = 0;
                while (bytes == 0)
                {
                    bytes = await stream.ReadAsync(dimBuffer, 0, 8);
                }

                var width = BitConverter.ToInt32(dimBuffer, 0);
                var height = BitConverter.ToInt32(dimBuffer, 4);
                Debug.Log($"Received dimensions: {width}, {height}");

                var buffer = new byte[width * height * 4];
                var offset = 0;
                while (buffer.Length != offset)
                {
                    var missingBytes = buffer.Length - offset;
                    var bytesToRead = Math.Min(Constants.BufferSize, missingBytes);
                    bytes = await stream.ReadAsync(buffer, offset, bytesToRead);
                    offset += bytes;
                }
                Debug.Log("Image read");

                // we are done with a packet
                // the texture is correct! it exports to the correct image
                image.texture = DataToTexture(width, height, buffer);
                _rect.sizeDelta = ExpandToRectSize(width, height);
            }
            
            Debug.LogWarning("Client loop has stopped!");
        }

        private void OnDisable()
        {
            _running = false;
        }

        private Vector2 ExpandToRectSize(int width, int height)
        {
            // currently only supports images that are taller than wider
            var aspect = (float)width / (float)height;
            var newWidth = aspect * _rectSize.y;
            return new Vector2(newWidth, _rectSize.y);
        }

        private static Texture2D DataToTexture(int width, int height, IReadOnlyList<byte> data)
        {
            var tex = new Texture2D(width, height);
            var colors = new Color32[data.Count / 4];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i].r = data[i * 4];
                colors[i].g = data[i * 4 + 1];
                colors[i].b = data[i * 4 + 2];
                colors[i].a = data[i * 4 + 3];
            }
            tex.SetPixels32(colors);
            tex.Apply();

            return tex;
        }
    }
}