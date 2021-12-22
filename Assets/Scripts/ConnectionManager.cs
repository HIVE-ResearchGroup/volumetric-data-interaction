using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// </summary>
public class ConnectionManager : MonoBehaviour
{

    public GameObject connectionButtonPanel;
    public Text log; 

    private UnetTransport transport;

    /// <summary>
    /// Connect Host if device has host IP
    /// Connect Client else
    /// </summary>
    public void Connect()
    {
        var ipAddress = System.Net.Dns.GetHostAddresses("");
        if (ipAddress.Length > 1 && ipAddress[1].ToString() == ConfigurationConstants.HOST_IP)
        {
            log.text = "HOST\n";
            Host();
        }
        else
        {
            log.text = "CLIENT\n";
            Join();
        }
    }

    /// <summary>
    /// Server only
    /// </summary>
    public void Host()
    {
        connectionButtonPanel.SetActive(false);

        transport = NetworkingManager.Singleton.GetComponent<UnetTransport>();
        transport.ConnectAddress = ConfigurationConstants.HOST_IP;
        transport.ConnectPort = ConfigurationConstants.DEFAULT_CONNECTING_PORT;

        NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkingManager.Singleton.StartHost(GetRandomSpawn(), Quaternion.identity);

        log.text += $"-HOST {transport.ConnectAddress}:{transport.ConnectPort}";
    }

    /// <summary>
    /// server only, PC
    /// </summary>
    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkingManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log("Approving a connection");
        callback(true, null, true, GetRandomSpawn(), Quaternion.identity);
    }

    /// <summary>
    /// Used for Tablet to connect
    /// </summary>
    public void Join()
    {
        var ipaddress = System.Net.Dns.GetHostAddresses("")[0];

        transport = NetworkingManager.Singleton.GetComponent<UnetTransport>();
        transport.ConnectAddress = ConfigurationConstants.HOST_IP;
        transport.ConnectPort = ConfigurationConstants.DEFAULT_CONNECTING_PORT;

        connectionButtonPanel.SetActive(false);
        NetworkingManager.Singleton.StartClient();
        log.text += $"-CLIENT {transport.ConnectAddress}:{transport.ConnectPort}";
    }

    private Vector3 GetRandomSpawn()
    {
        var x = Random.Range(-5f, 5f);
        var y = Random.Range(-1f, 1f);
        var z = Random.Range(-5f, 5f);
        return new Vector3(x, y, z);
    }
}
