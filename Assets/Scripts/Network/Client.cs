using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// </summary>
public class Client : ConnectionManager
{
    public Menu Menu;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }

    public void Update()
    {
        UpdateMessagePump();
    }

    protected override void Init()
    {
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc, 1);
        hostId = NetworkTransport.AddHost(topo, 0);

        connectionId = NetworkTransport.Connect(hostId, ConfigurationConstants.HOST_IP, ConfigurationConstants.DEFAULT_CONNECTING_PORT, 0, out error);
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
        switch (type)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connected ");
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnected");
                break;
            case NetworkEventType.DataEvent:
                Debug.Log("Received Data");
                break;
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected data");
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

        HandleMessageContent(message);
        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
    }

    /// <summary>
    /// Apply input which is necessary for the client directly
    /// </summary>
    private void HandleMessageContent(NetworkMessage msg)
    {
        switch (msg.OperationCode)
        {
            case NetworkOperationCode.Swipe:
                var swipeMsg = (SwipeMessage)msg;

                if (swipeMsg.IsInwardSwipe)
                {
                    Menu.Cancel();
                }
                break;
        }
    }
}
