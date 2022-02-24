using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// </summary>
public class ConnectionManager : MonoBehaviour
{

    public GameObject ConnectionUI;
    public GameObject InteractionUI;
    public Text Log; 

    private UnetTransport transport;

    private bool isConnected = false;
    private bool isWorkStation;

    /// <summary>
    /// Connect Host if device has host IP
    /// Connect Client else
    /// </summary>
    public void Connect()
    {
        ConnectionUI.SetActive(false);
        InteractionUI.SetActive(true);
        
        if (isWorkStation)
        {
            Log.text = "HOST";
            Host();
            isConnected = true;
        }
        else
        {
            Log.text = "CLIENT";
            Join();
            isConnected = true;
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
        Log.text = $"-HOST {transport.ConnectAddress}:{transport.ConnectPort}"; 

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
        transport = NetworkingManager.Singleton.GetComponent<UnetTransport>();
        transport.ConnectAddress = ConfigurationConstants.HOST_IP;
        transport.ConnectPort = ConfigurationConstants.DEFAULT_CONNECTING_PORT;
        Log.text = $"-CLIENT: Connected to {transport.ConnectAddress}:{transport.ConnectPort}\n";

        NetworkingManager.Singleton.StartClient();
    }

    public void Reconnect()
    {
        Log.text = "Reconnecting... ";
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

        if (!isConnected) //automatically connect host so only client needs to join manually
        {
            var ipAddress = System.Net.Dns.GetHostAddresses("");
            isWorkStation = ipAddress.Length > 1 && ipAddress[1].ToString() == ConfigurationConstants.HOST_IP;

            if (isWorkStation)
            {
                Connect();
            }
        }
    }
}
