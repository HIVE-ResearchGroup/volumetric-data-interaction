#nullable enable

using Extensions;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Networking.Tablet
{
    public class TabletClient : MonoBehaviour
    {
        [SerializeField]
        private string ip = "127.0.0.1";

        [SerializeField]
        private int port = Ports.TabletPort;

        public string IP
        {
            get => ip;
            set => ip = value;
        }

        public int Port
        {
            get => port;
            set => port = value;
        }

        private Thread? receivingThread;

        private TcpClient tcpClient = null!;
        private NetworkStream stream = null!;

        public event Action? Connected;

        public event Action? ModelSelected;

        public event Action? SnapshotSelected;

        public event Action? SnapshotRemoved;

        private void Awake()
        {
            tcpClient = new TcpClient();
        }

        private void OnDestroy()
        {
            tcpClient.Close();
            receivingThread?.Join();
        }

        public void Connect()
        {
            receivingThread = new Thread(() =>
            {
                Debug.Log("Trying to connect");
                tcpClient.Connect(IP, Port);
                Debug.Log("Getting Stream");
                stream = tcpClient.GetStream();
                Debug.Log("Connected to server");

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Connected?.Invoke();
                });

                var buffer = new byte[1];
                while (true)
                {
                    try
                    {
                        stream.ReadAll(buffer, 0, 1);
                    }
                    catch
                    {
                        break;
                    }
                    switch (buffer[0])
                    {
                        case Categories.SelectedModel:
                            UnityMainThreadDispatcher.Instance().Enqueue(() => ModelSelected?.Invoke());
                            break;
                        case Categories.SelectedSnapshot:
                            UnityMainThreadDispatcher.Instance().Enqueue(() => SnapshotSelected?.Invoke());
                            break;
                        case Categories.SnapshotRemoved:
                            UnityMainThreadDispatcher.Instance().Enqueue(() => SnapshotRemoved?.Invoke());
                            break;
                        default:
                            Debug.LogWarning("Unsupported command was received!");
                            break;
                    }
                }
            });
            receivingThread.Start();
            Debug.Log("Thread started");
        }

        public void Send(ICommand cmd)
        {
            stream.Write(cmd.ToByteArray());
        }
        
        public void Send(byte command)
        {
            stream.WriteByte(command);
        }
    }
}
