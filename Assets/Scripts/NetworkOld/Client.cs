using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Constants;
using Interaction;
using NetworkOld.Message;
using UnityEngine;
using UnityEngine.Networking;

namespace NetworkOld
{
    /// <summary>
    /// Firewall for Domain Network needs to be deactivated!
    /// </summary>
    public class Client : ConnectionManager
    {
        [SerializeField]
        private Menu menu;
        [SerializeField]
        private string ip = ConfigurationConstants.DEFAULT_IP;
        [SerializeField]
        private int port = ConfigurationConstants.DEFAULT_PORT;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            Init(ConnectionType.Client);
            Connect(ip, port);
        }

        public void Update()
        {
            UpdateMessagePump();
        }

        public void SetIPAndReconnect(string ip)
        {
            this.ip = ip;
            Connect(ip, port);
        }

        public override void UpdateMessagePump()
        {
            int recHostId;
            int connectionId;
            int channelId;

            byte[] recBuffer = new byte[BYTE_SIZE];
            int dataSize;
            byte error;

            NetworkEventType type = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, BYTE_SIZE, out dataSize, out error);
            if (error != 0)
            {
                Debug.LogError($"Error: {(NetworkError)error}");
            }
            switch (type)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    Debug.Log("Connected");
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log("Disconnected");
                    break;
                case NetworkEventType.DataEvent:
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream(recBuffer);
                    NetworkMessage msg = (NetworkMessage)formatter.Deserialize(ms);
                    Debug.Log($"Data Event: {msg}");

                    OnData(connectionId, channelId, recHostId, msg);
                    break;
                default:
                case NetworkEventType.BroadcastEvent:
                    Debug.Log("Unexpected data");
                    break;
            }
        }

        private void OnData(int connectionId, int channelId, int recHostId, NetworkMessage msg)
        {
            switch ((NetworkOperationCode)msg.OperationCode)
            {
                case NetworkOperationCode.None:
                    break;            
                case NetworkOperationCode.MenuMode:
                    var modeMessage = (ModeMessage)msg;

                    if ((MenuMode)modeMessage.Mode == MenuMode.Selected)
                    {
                        menu.SelectedObject();
                    }
                    else if ((MenuMode)modeMessage.Mode == MenuMode.None)
                    {
                        menu.Cancel();
                    }
                    break;
                case NetworkOperationCode.Text:
                    var textMessage = (TextMessage)msg;
                    menu.SendDebug(textMessage.Text);
                    break;
            }
        }

        public void SendServer(NetworkMessage message)
        {
            byte[] buffer = new byte[BYTE_SIZE];

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);

            try
            {
                formatter.Serialize(ms, message);
            }
            catch (Exception e)
            {
                Debug.Log($"Message not serializable! Error: {e.Message}");
                return;
            }

            NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
            HandleMessageContent(message);
        }

        /// <summary>
        /// Apply input which is necessary for the client directly
        /// </summary>
        private void HandleMessageContent(NetworkMessage msg)
        {
            switch ((NetworkOperationCode)msg.OperationCode)
            {
                case NetworkOperationCode.Swipe:
                    var swipeMsg = (SwipeMessage)msg;

                    if (swipeMsg.IsInwardSwipe)
                    {
                        SendServer(new TextMessage("Cancel initiated from client"));
                        menu.Cancel();
                    }
                    break;
                case NetworkOperationCode.Tab:
                    var tabMsg = (TabMessage)msg;
                    if (tabMsg.TabType == (int)TabType.HoldStart)
                    {
                        SendServer(new TextMessage("Hold Start initiated from client"));
                        menu.StartMapping();
                    }
                    else if (tabMsg.TabType == (int)TabType.HoldEnd)
                    {
                        SendServer(new TextMessage("Hold End initiated from client"));
                        menu.StopMapping();
                    }
                    break;
            }
        }
    }
}
