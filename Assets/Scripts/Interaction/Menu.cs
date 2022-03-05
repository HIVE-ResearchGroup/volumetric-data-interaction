using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu class
/// Toggle between menu and detail view
/// </summary>
public class Menu : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject InteractionMenu;
    public Text ModeTitle;

    public MenuMode Mode;
       
    void Start()
    {
        Mode = MenuMode.None;        
    }

    public void StartSelection()
    {
        Debug.Log("Selection");
        Mode = MenuMode.Selection;
        SendToHost(new ModeMessage(Mode));

        MainMenu.SetActive(false);
        InteractionMenu.SetActive(true);
        ModeTitle.text = "Selection Mode";
    }

    public void SelectedObject()
    {
        // set object as gameobject in a specific script?
        Mode = MenuMode.Selected;
        SendToHost(new ModeMessage(Mode));
        SendToHost(new TextMessage("Selected"));
        ModeTitle.text = "Selected Mode";
    }

    public void StartMapping()
    {
        Mode = MenuMode.Mapping;
        SendToHost(new ModeMessage(Mode));
        SendToHost(new TextMessage("Start mapping"));
        ModeTitle.text = "Mapping Mode";
    }

    public void StopMapping()
    {
        Mode = MenuMode.Selected;
        SendToHost(new ModeMessage(Mode));
        SendToHost(new TextMessage("Stop mapping"));
        ModeTitle.text = "Selected Mode";
    }

    public void StartAnalysis()
    {
        Mode = MenuMode.Analysis;
        SendToHost(new ModeMessage(Mode));
        MainMenu.SetActive(false);
        InteractionMenu.SetActive(true);
        ModeTitle.text = "Analysis Mode";
    }

    public void Cancel()
    {
        Mode = MenuMode.None;
        SendToHost(new ModeMessage(Mode));
        MainMenu.SetActive(true);
        InteractionMenu.SetActive(false);
    }

    public void SendDebug(string text)
    {
        SendToHost(new TextMessage(text));

        Mode = MenuMode.Mapping;
        MainMenu.SetActive(false);
        InteractionMenu.SetActive(true);
        ModeTitle.text = text;
    }

    private void SendToHost(NetworkMessage message)
    {
        var client = GameObject.Find(StringConstants.Client)?.GetComponent<Client>();
        client?.SendServer(message);
    }
      
}
