using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// Start Host before connecting Client
/// </summary>
public class HostConnectionManager : MonoBehaviour
{
    private UnetTransport transport;
    private bool isConnected = false;

    public void Connect()
    {
        transport = NetworkingManager.Singleton.GetComponent<UnetTransport>();
        transport.ConnectAddress = ConfigurationConstants.HOST_IP;
        transport.ConnectPort = ConfigurationConstants.DEFAULT_CONNECTING_PORT;

        NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkingManager.Singleton.StartHost(GetRandomSpawn(), Quaternion.identity);
        Debug.Log($"-HOST {transport.ConnectAddress}:{transport.ConnectPort}");
        isConnected = true;
    }

    /// <summary>
    /// server only, PC
    /// </summary>
    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkingManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log($"Approving a connection from {clientID}");
        callback(true, null, true, GetRandomSpawn(), Quaternion.identity);
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
    }
}
