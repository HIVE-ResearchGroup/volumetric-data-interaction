using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// </summary>
public class Host : ConnectionManager
{
    private MenuMode MenuMode;

    public GameObject SelectedObject;
    public GameObject HighlightedObject;
    private GameObject ray;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }

    void Update()
    {
        UpdateMessagePump();

        if (Input.GetKeyDown(KeyCode.R))
        {
            MenuMode = MenuMode.Selection; //for debugging
            var overlayScreen = GameObject.Find(StringConstants.OverlayScreen);
            var rayPrefab = Resources.Load(StringConstants.PrefabRay, typeof(GameObject)) as GameObject;
            ray = Instantiate(rayPrefab, overlayScreen.transform);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            HandleTab(TabType.Double);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            SendClient(new TextMessage("Tester123"));
        }
    }

    protected override void Init()
    {
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc, 1);

        hostId = NetworkTransport.AddHost(topo, ConfigurationConstants.DEFAULT_CONNECTING_PORT, null);

        connectionId = NetworkTransport.Connect(hostId, ConfigurationConstants.DEFAULT_IP, ConfigurationConstants.DEFAULT_CONNECTING_PORT, 0, out error);
    }
             
    public override void UpdateMessagePump()
    {
        var byteSize = 1024;
        int recHostId;
        int connectionId;
        int channelId;

        byte[] recBuffer = new byte[byteSize];
        int dataSize;
        byte error;

        NetworkEventType type = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, byteSize, out dataSize, out error);
        switch (type)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("User {0} has connected!", connectionId));
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("User {0} has disconnected!", connectionId));
                break;
            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetworkMessage msg = (NetworkMessage)formatter.Deserialize(ms);

                OnData(connectionId, channelId, recHostId, msg);
                break;
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Broadcast - save Hawaii");
                break;
        }
    }

    private void OnData(int connectionId, int channelId, int recHostId, NetworkMessage msg)
    {
        switch (msg.OperationCode)
        {
            case NetworkOperationCode.None:
                break;
            case NetworkOperationCode.Shake:
                var shakeMsg = (ShakeMessage)msg;
                Debug.Log($"Shake detected - count {shakeMsg.Count}");
                //TODO - react to shakes
                break;
            case NetworkOperationCode.Tilt:
                var tiltMsg = (TiltMessage)msg;
                Debug.Log($"Tilt detected - isLeft {tiltMsg.IsLeft}");
                //TODO - react to tilt
                break;
            case NetworkOperationCode.Tab:
                var tabMsg = (TabMessage)msg;
                Debug.Log("Tab type: " + tabMsg.TabType);

                HandleTab((TabType)tabMsg.TabType);
                break;
            case NetworkOperationCode.Swipe:
                var swipeMsg = (SwipeMessage)msg;
                Debug.Log("Swipe detected: inward = " + swipeMsg.IsInwardSwipe);

                if (!swipeMsg.IsInwardSwipe && MenuMode == MenuMode.Analysis)
                {
                    // TODO - place snapshot
                    Debug.Log("place snapshot - to be done");
                }
                break;
            case NetworkOperationCode.Scale:
                var scaleMessage = (ScaleMessage)msg;
                SelectedObject.transform.localScale *= scaleMessage.ScaleMultiplier;
                // TODO - react to scaling or grab depending on mode
                break;
            case NetworkOperationCode.Rotation:
                var rotationmessage = (RotationMessage)msg;
                SelectedObject.transform.Rotate(0.0f, 0.0f, rotationmessage.RotationRadiansDelta * Mathf.Rad2Deg);
                // TODO - react to rotation
                break;
            case NetworkOperationCode.MenuMode:
                var modeMessage = (ModeMessage)msg;
                HandleModeChange(MenuMode, (MenuMode)modeMessage.Mode);
                MenuMode = (MenuMode)modeMessage.Mode;
                break;
            case NetworkOperationCode.Text:
                var textMsg = (TextMessage)msg;
                Debug.Log("Debug: " + textMsg.Text);
                break;
        }
    }

    public void SendClient(NetworkMessage message)
    {
        byte[] buffer = new byte[BYTE_SIZE];

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);

        try
        {
            formatter.Serialize(ms, message);
        }
        catch (Exception e)
        {
            Debug.Log($"Message not serializable! Error: {e.Message}");
            return;
        }

        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
    }

    private void HandleTab(TabType type)
    {
        switch(type)
        {
            case TabType.Single:
                // no need for action?
                break;
            case TabType.Double:
                if (MenuMode == MenuMode.Selection && HighlightedObject != null)
                {
                    Debug.Log("select highlighted object");
                    SelectedObject = HighlightedObject;
                    var greenMaterial = Resources.Load(StringConstants.MateriaGreen, typeof(Material)) as Material;
                    SelectedObject.GetComponent<MeshRenderer>().material = greenMaterial;

                    Destroy(ray);
                    HighlightedObject = null;
                    SendClient(new ModeMessage(MenuMode.Selected));
                }
                break;
            case TabType.HoldStart:
                break;
            case TabType.HoldEnd:
                break;
        }
    }

    private void HandleModeChange(MenuMode prevMode, MenuMode currMode)
    {
        if (prevMode == currMode)
        {
            return;
        }

        if (currMode == MenuMode.Selection)
        {
            Debug.Log("Selection started");
            var overlayScreen = GameObject.Find(StringConstants.OverlayScreen);
            var rayPrefab = Resources.Load(StringConstants.PrefabRay, typeof(GameObject)) as GameObject;
            ray = Instantiate(rayPrefab, overlayScreen.transform);
        }

        if (currMode == MenuMode.None)
        {
            // cancel selection mode
            SelectedObject = null;
            HighlightedObject = null;
            ray = null;
        }
    }
}
