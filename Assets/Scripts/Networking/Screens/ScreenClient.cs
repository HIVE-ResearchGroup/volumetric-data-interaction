#nullable enable

using System.Net.Sockets;
using System.Threading;
using Extensions;
using PimDeWitte.UnityMainThreadDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking.Screens
{
    public class ScreenClient : MonoBehaviour
    {
        [SerializeField]
        private int port = Ports.ScreenPort;

        [SerializeField]
        private RectTransform uiTransform = null!;
        
        [SerializeField]
        private RawImage image = null!;

        [SerializeField]
        private GameObject networkConfig = null!;

        [SerializeField]
        private TMP_InputField ipInput = null!;

        [SerializeField]
        private TMP_InputField idInput = null!;

        private Thread? receivingThread;
        
        private TcpClient client = null!;
        
        private RectTransform rect = null!;

        private Vector2 uiSize;

        private void Awake()
        {
            client = new TcpClient();
            rect = image.GetComponent<RectTransform>();
            uiSize = uiTransform.sizeDelta;
        }

        private void OnDestroy()
        {
            client.Close();
            receivingThread?.Join();
        }

        public void OnConnectClicked()
        {
            var ip = ipInput.text;
            if (!int.TryParse(idInput.text, out var id))
            {
                Debug.LogError("Couldn't parse ID!");
                return;
            }

            receivingThread = new Thread(() =>
            {
                client.Connect(ip, port);
                using var stream = client.GetStream();

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    networkConfig.SetActive(false);
                    image.gameObject.SetActive(true);
                });

                stream.Write(new IDAdvertisement(id).ToByteArray());
                Debug.Log($"ID sent {id}");

                var dimBuffer = new byte[Dimensions.Size];
            
                while (true)
                {
                    try
                    {
                        stream.ReadAll(dimBuffer, 0, Dimensions.Size);
                    }
                    catch
                    {
                        break;
                    }

                    var dims = Dimensions.FromByteArray(dimBuffer);
                    Debug.Log($"Received dimensions: {dims.Width}, {dims.Height}");

                    var buffer = new byte[ImageData.GetBufferSize(dims)];
                    try
                    {
                        stream.ReadAll(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        break;
                    }
                    Debug.Log("Image read");
                    var imageData = ImageData.FromByteArray(buffer);

                    var size = ExpandToRectSize(dims.Width, dims.Height);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        image.color = Color.white;
                        image.texture = DataToTexture(dims, imageData);
                        rect.sizeDelta = size;
                    });
                }
            
                Debug.LogWarning("Client loop has stopped!");
            });
            receivingThread.Start();
        }

        private Vector2 ExpandToRectSize(int width, int height)
        {
            var aspect = width / (float)height;
            if (aspect < 1)
            {
                var newWidth = uiSize.y * aspect;
                return new Vector2(newWidth, uiSize.y);
            }
            else
            {
                var newHeight = uiSize.x / aspect;
                return new Vector2(uiSize.x, newHeight);
            }
        }

        private static Texture2D DataToTexture(Dimensions dims, ImageData data)
        {
            var tex = new Texture2D(dims.Width, dims.Height);
            tex.SetPixels32(data.Data);
            tex.Apply();
            return tex;
        }
    }
}