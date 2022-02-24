using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// Start Host before connecting Client
/// </summary>
public class HostConnectionManager : MonoBehaviour
{
    private UnetTransport transport;
    private bool isConnected = false;

    private int BYTE_SIZE = 1024;

    private int hostId;
    private int connectionId;
    private byte reliableChannel;
    private byte error;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }
    void Update()
    {
        UpdateMessagePump();
    }

    private void Init()
    {
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc, 1);

        hostId = NetworkTransport.AddHost(topo, ConfigurationConstants.DEFAULT_CONNECTING_PORT, null);

        NetworkTransport.Connect(hostId, ConfigurationConstants.DEFAULT_IP, ConfigurationConstants.DEFAULT_CONNECTING_PORT, 0, out error);

        Debug.Log($"Connected to Host {hostId}");
        isConnected = true;
    }

    public void Shutdown()
    {
        NetworkTransport.Shutdown();
    }
             
    public void UpdateMessagePump()
    {
        var byteSize = 1024;
        int recHostId;
        int connectionId;
        int channelId;

        byte[] recBuffer = new byte[byteSize];
        int dataSize;
        byte error;

        NetworkEventType type = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, byteSize, out dataSize, out error);
        switch (type)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("User {0} has connected!", connectionId));
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("User {0} has disconnected!", connectionId));
                break;
            case NetworkEventType.DataEvent:
                Debug.Log("Data");
                break;
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Broadcast - save Hawaii");
                break;
        }
    }
}
