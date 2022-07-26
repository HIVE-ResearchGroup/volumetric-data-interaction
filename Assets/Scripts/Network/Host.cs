using Assets.Scripts.Exploration;
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
    public GameObject Tracker;
    public GameObject SelectedObject;
    public GameObject HighlightedObject;

    public Slicer Slicer;
    private MenuMode MenuMode;
    private InterfaceVisualisation ui;

    private GameObject ray;
    private GameObject overlayScreen;

    private Exploration analysis;
    private SpatialInteraction spatialHandler;
    private SnapshotInteraction snapshotHandler;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
        overlayScreen = GameObject.Find(StringConstants.OverlayScreen);

        analysis = new Exploration(Tracker);
        ui = gameObject.AddComponent<InterfaceVisualisation>();

        spatialHandler = gameObject.AddComponent<SpatialInteraction>();
        spatialHandler.Tracker = Tracker;
        snapshotHandler = gameObject.AddComponent<SnapshotInteraction>();
        snapshotHandler.Tracker = Tracker;
    }

    void Update()
    {
        UpdateMessagePump();
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
                HandleShakes(shakeMsg.Count);
                break;
            case NetworkOperationCode.Tilt:
                var tiltMsg = (TiltMessage)msg;
                Debug.Log($"Tilt detected - isLeft {tiltMsg.IsLeft}");
                if (MenuMode == MenuMode.Selected)
                    snapshotHandler.GetNeighbour(tiltMsg.IsLeft, SelectedObject);
                break;
            case NetworkOperationCode.Tab:
                var tabMsg = (TabMessage)msg;
                Debug.Log("Tab type: " + tabMsg.TabType);
                HandleTab((TabType)tabMsg.TabType);
                break;
            case NetworkOperationCode.Swipe:
                var swipeMsg = (SwipeMessage)msg;
                HandleSwipe(swipeMsg.IsInwardSwipe, swipeMsg.EndPointX, swipeMsg.EndPointY, (float)swipeMsg.Angle);
                break;
            case NetworkOperationCode.Scale:
                var scaleMessage = (ScaleMessage)msg;
                HandleScaling(scaleMessage.ScaleMultiplier);
                break;
            case NetworkOperationCode.Rotation:
                var rotationmessage = (RotationMessage)msg;
                spatialHandler.HandleRotation(rotationmessage.RotationRadiansDelta, SelectedObject);
                break;
            case NetworkOperationCode.MenuMode:
                var modeMessage = (ModeMessage)msg;
                HandleModeChange(MenuMode, (MenuMode)modeMessage.Mode);
                MenuMode = (MenuMode)modeMessage.Mode;
                break;
            case NetworkOperationCode.Text:
                var textMsg = (TextMessage)msg;
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

    #region Input Handling
    private void HandleShakes(int shakeCount)
    {
        if (shakeCount < 1) // one shake can happen unintentionally
        {
            return;
        }


        var hasDeleted = snapshotHandler.DeleteSnapshotsIfExist(SelectedObject, shakeCount);
        if (!hasDeleted && shakeCount > 1)
        {
            analysis.ResetModel();
        }

        HandleModeChange(MenuMode, MenuMode.None);
        SendClient(new ModeMessage(MenuMode.None));
    }

    private void HandleTab(TabType type)
    {
        switch(type)
        {
            case TabType.Single:
                break;
            case TabType.Double:
                if (MenuMode == MenuMode.Selection && HighlightedObject != null)
                {
                    SelectedObject = HighlightedObject;
                    SelectedObject.GetComponent<Selectable>()?.SetToSelected();

                    Destroy(ray);
                    HighlightedObject = null;

                    SelectedObject.GetComponent<Snapshot>()?.SetSelected(true);

                    SendClient(new ModeMessage(MenuMode.Selected));
                }
                else if (MenuMode == MenuMode.Analysis)
                {
                    Slicer.TriggerSlicing();
                }
                break;
            case TabType.HoldStart:
                spatialHandler.StartMapping(SelectedObject);
                break;
            case TabType.HoldEnd:
                spatialHandler.StopMapping(SelectedObject);
                break;
        }
    }

    private void HandleSwipe(bool isSwipeInward, float endX, float endY, float angle)
    {
        if (isSwipeInward)
        {
            return;
        }

        if (MenuMode == MenuMode.Analysis)
        {
            snapshotHandler.HandleSnapshotCreation(angle);
        }
    }

    /// <summary>
    /// Depending on mode, the scale input is used for resizing object or recognised as a grab gesture
    /// </summary>
    private void HandleScaling(float scaleMultiplier)
    {
        if(MenuMode == MenuMode.Selected)
        {
            SelectedObject.transform.localScale *= scaleMultiplier;
        }
        else if (SelectedObject == null)
        {
            snapshotHandler.AlignOrMisAlignSnapshots();
        }
    }

    private void HandleModeChange(MenuMode prevMode, MenuMode currMode)
    {
        if (prevMode == currMode)
        {
            return;
        }

        var isSnapshotSelected = false;
        switch(currMode)
        {
            case MenuMode.None:
                if (prevMode == MenuMode.Analysis)
                {
                    Slicer.SetActive(false);
                }
                else
                {
                    ResetFromSelectionMode();
                }

                ui.SetHUD(StringConstants.MainModeInfo);
                ui.SetCenterText(StringConstants.MainModeInfo);
                break;
            case MenuMode.Selection:
                ui.SetHUD(StringConstants.SelectionModeInfo);
                ui.SetCenterText(StringConstants.SelectionModeInfo);

                var overlayScreen = GameObject.Find(StringConstants.Main);
                var rayPrefab = Resources.Load(StringConstants.PrefabRay, typeof(GameObject)) as GameObject;
                ray = Instantiate(rayPrefab, overlayScreen.transform);
                break;
            case MenuMode.Selected:
                isSnapshotSelected = snapshotHandler.IsSnapshot(SelectedObject);
                break;
            case MenuMode.Analysis:
                ui.SetHUD(StringConstants.ExplorationModeInfo);
                ui.SetCenterText(StringConstants.ExplorationModeInfo);
                Slicer.SetActive(true);
                break;
        }

        ui.SetMode(currMode, isSnapshotSelected);
    }

    private void ResetFromSelectionMode()
    {
        if (ray)
        {
            Destroy(ray);
            ray = null;
        }

        if (HighlightedObject != null || SelectedObject != null)
        {
            UnselectObject();
            snapshotHandler.CleanUpNeighbours();
            snapshotHandler.DeactivateAllSnapshots();
        }
    }

    private void UnselectObject()
    {
        var activeObject = HighlightedObject ?? SelectedObject;
        Selectable selectable = activeObject.GetComponent<Selectable>();
        if (selectable)
        {
            selectable.SetToDefault();
            SelectedObject = null;
            HighlightedObject = null;
        }

        activeObject.GetComponent<Snapshot>()?.SetSelected(false);
        GameObject.Find(StringConstants.Main).GetComponent<MeshRenderer>().material.mainTexture = null;
    }

    public void ChangeSelectedObject(GameObject newObject)
    {
        UnselectObject();
        SelectedObject = newObject;
    }
    #endregion //input handling
}
