using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{

    public GameObject connectionButtonPanel;

    private UnetTransport transport;
    /// <summary>
    /// Server only
    /// </summary>
    public void Host()
    {
        connectionButtonPanel.SetActive(false);

        transport = NetworkingManager.Singleton.GetComponent<UnetTransport>();
        transport.ConnectAddress = ConfigurationConstants.DEFAULT_IP;
        NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkingManager.Singleton.StartHost(GetRandomSpawn(), Quaternion.identity);
    }

    /// <summary>
    /// server only
    /// </summary>
    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkingManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log("Approving a connection");
        callback(true, null, true, GetRandomSpawn(), Quaternion.identity);
    }

    public void Join()
    {
        transport = NetworkingManager.Singleton.GetComponent<UnetTransport>();
        transport.ConnectAddress = ConfigurationConstants.DEFAULT_IP;
        //transport.ConnectPort = ConfigurationConstants.DEFAULT_CONNECTING_PORT;

        connectionButtonPanel.SetActive(false);
        NetworkingManager.Singleton.StartClient();
    }

    private Vector3 GetRandomSpawn()
    {
        var x = Random.Range(-5f, 5f);
        var y = Random.Range(-1f, 1f);
        var z = Random.Range(-5f, 5f);
        return new Vector3(x, y, z);
    }
}
