using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// </summary>
public abstract class ConnectionManager : MonoBehaviour
{
    protected int BYTE_SIZE = 1024;

    protected int hostId;
    protected int connectionId;
    protected byte reliableChannel;
    protected byte error;

    protected abstract void Init();

    public void Shutdown()
    {
        NetworkTransport.Shutdown();
    }

    public abstract void UpdateMessagePump();
}
