using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
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
}
