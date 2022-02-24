using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// </summary>
public class Client : ConnectionManager
{
    public Text Log;

    public GameObject ConnectionUI;
    public GameObject InteractionUI;

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

        ConnectionUI.SetActive(false);
        InteractionUI.SetActive(true);
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
                Log.text += "Connected ";
                break;
            case NetworkEventType.DisconnectEvent:
                Log.text += "Disconnected ";
                break;
            case NetworkEventType.DataEvent:
                Log.text += "Data -- ";
                break;
            default:
            case NetworkEventType.BroadcastEvent:
                Log.text += "Unexpected data";
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
            Log.text = $"Message not serializable! Error: {e.Message}";
            return;
        }

        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
    }
}
