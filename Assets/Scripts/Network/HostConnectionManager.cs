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

    private ulong currentClientId;

    public void Connect()
    {
        transport = NetworkingManager.Singleton.GetComponent<UnetTransport>();
        transport.ConnectAddress = ConfigurationConstants.HOST_IP;
        transport.ConnectPort = ConfigurationConstants.DEFAULT_CONNECTING_PORT;

        //NetworkingManager.Singleton.BroadcastMessage // to send image?

        NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkingManager.Singleton.StartHost(GetRandomSpawn(), Quaternion.identity);
        Debug.Log($"-HOST ({transport.GetInstanceID()} - {transport.ServerClientId}) {transport.ConnectAddress}:{transport.ConnectPort}");
        isConnected = true;
    }

    /// <summary>
    /// server only, PC
    /// </summary>
    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkingManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log($"Approving a connection from {clientID}");
        callback(true, null, true, GetRandomSpawn(), Quaternion.identity);
        currentClientId = clientID;
    }
      
    private Vector3 GetRandomSpawn()
    {
        var x = Random.Range(-5f, 5f);
        var y = Random.Range(-1f, 1f);
        var z = Random.Range(-5f, 5f);
        return new Vector3(x, y, z);
    }

    void Update()
    {
        if (!isConnected)
        {
            Connect();
        }
       
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
