using MLAPI;
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

    public void Join()
    {
        transport = NetworkingManager.Singleton.GetComponent<UnetTransport>();
        transport.ConnectAddress = ConfigurationConstants.HOST_IP;
        transport.ConnectPort = ConfigurationConstants.DEFAULT_CONNECTING_PORT;
        Log.text = $"-CLIENT: Connected to {transport.ConnectAddress}:{transport.ConnectPort}\n";

        NetworkingManager.Singleton.StartClient();
        ConnectionUI.SetActive(false);
        InteractionUI.SetActive(true);
    }

    public void Reconnect()
    {
        transport.Send(transport.ServerClientId, new System.ArraySegment<byte>(), "test");

        Log.text = $"Server: {transport.ServerClientId} - { NetworkingManager.Singleton.ConnectedHostname}\n";
        Log.text += "Reconnecting... ";
        try
        {
            NetworkingManager.Singleton.StopClient();
        }
        catch (System.Exception e)
        {
            //log.text += $"Exception thrown: {e.Message}";
        }

        Join();
    }

    public void Update()
    {
        UpdateMessagePump();
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
}
