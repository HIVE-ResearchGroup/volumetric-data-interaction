using MLAPI;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{

    public GameObject connectionButtonPanel;

    /// <summary>
    /// Server only
    /// </summary>
    public void Host()
    {
        connectionButtonPanel.SetActive(false);
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
        connectionButtonPanel.SetActive(false);
        NetworkingManager.Singleton.StartClient();
    }

    private Vector3 GetRandomSpawn()
    {
        var x = Random.Range(-10f, 10f);
        var y = Random.Range(-10f, 10f);
        var z = Random.Range(-10f, 10f);
        return new Vector3(x, y, z);
    }
}
