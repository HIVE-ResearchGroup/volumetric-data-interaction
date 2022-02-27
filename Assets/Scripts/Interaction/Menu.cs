using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu class
/// Toggle between menu and detail view
/// </summary>
public class Menu : MonoBehaviour
{
    public Text ModeTitle;
    public Text Log;

    public MenuMode Mode;
       
    void Start()
    {
        Mode = MenuMode.None;        
    }

    void Update()
    {
       
    }

    private void SendToClient(NetworkMessage message)
    {
        var client = GameObject.Find(StringConstants.Client)?.GetComponent<Client>();
        client?.SendServer(message);
    }
      
}
