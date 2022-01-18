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
    public GameObject sideMenu;
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
            sideMenu.SetActive(true);
            connectionButtonPanel.SetActive(false);
            Host();
        }
        else
        {
            log.text = "CLIENT\n";
            sideMenu.SetActive(true);
            connectionButtonPanel.SetActive(false);
            Join();
        }
    }

    /// <summary>
    /// Server only
    /// </summary>
    public void Host()
    {
        transport = NetworkingManager.Singleton.GetComponent<UnetTransport>();
        transport.ConnectAddress = ConfigurationConstants.HOST_IP;
        transport.ConnectPort = ConfigurationConstants.DEFAULT_CONNECTING_PORT;
        log.text = $"-HOST {transport.ConnectAddress}:{transport.ConnectPort}\n"; 

        NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkingManager.Singleton.StartHost(GetRandomSpawn(), Quaternion.identity);

    }

    /// <summary>
    /// server only, PC
    /// </summary>
    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkingManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log($"Approving a connection from {clientID}");
        callback(true, null, true, GetRandomSpawn(), Quaternion.identity);
    }

    /// <summary>
    /// Used for Tablet to connect
    /// </summary>
    public void Join()
    {
        log.text += "Try join as client \n";
        transport = NetworkingManager.Singleton.GetComponent<UnetTransport>();
        transport.ConnectAddress = ConfigurationConstants.HOST_IP;
        transport.ConnectPort = ConfigurationConstants.DEFAULT_CONNECTING_PORT;
        log.text += $"-CLIENT: Connected to {transport.ConnectAddress}:{transport.ConnectPort}\n";

        NetworkingManager.Singleton.StartClient();
    }

    public void Reconnect()
    {
        log.text = "Reconnecting... \n";
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

    private Vector3 GetRandomSpawn()
    {
        var x = Random.Range(-5f, 5f);
        var y = Random.Range(-1f, 1f);
        var z = Random.Range(-5f, 5f);
        return new Vector3(x, y, z);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.H))
        {
            Host();
        }
    }
}
