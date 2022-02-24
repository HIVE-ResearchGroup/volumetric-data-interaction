using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// Start Host before connecting Client
/// </summary>
public class ClientConnectionManager : MonoBehaviour
{
    public GameObject ConnectionUI;
    public GameObject InteractionUI;
    public Text Log; 

    private UnetTransport transport;
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

    public void Update()
    {
        UpdateMessagePump();
    }

    private void Init()
    {
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc, 1);
        hostId = NetworkTransport.AddHost(topo, 0);

        NetworkTransport.Connect(hostId, ConfigurationConstants.HOST_IP, ConfigurationConstants.DEFAULT_CONNECTING_PORT, 0, out error);

        ConnectionUI.SetActive(false);
        InteractionUI.SetActive(true);
    }

    public void Shutdown()
    {
        NetworkTransport.Shutdown();
    }


    public void UpdateMessagePump()
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

    #region Send

    public void SendServer()
    {
        byte[] buffer = new byte[BYTE_SIZE];

        //TODO
    }

    #endregion
}
