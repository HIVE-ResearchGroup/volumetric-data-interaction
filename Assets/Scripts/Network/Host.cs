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
    public GameObject tracker;
    public GameObject selectedObject;
    public GameObject highlightedObject;

    public Slicer slicer;
    private MenuMode menuMode;
    private InterfaceVisualisation ui;

    private GameObject ray;

    private Exploration analysis;
    private SpatialInteraction spatialHandler;
    private SnapshotInteraction snapshotHandler;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();

        analysis = gameObject.AddComponent<Exploration>();
        analysis.tracker = tracker;
        ui = gameObject.AddComponent<InterfaceVisualisation>();

        spatialHandler = gameObject.AddComponent<SpatialInteraction>();
        spatialHandler.tracker = tracker;
        snapshotHandler = gameObject.AddComponent<SnapshotInteraction>();
        snapshotHandler.tracker = tracker;
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
        int recHostId;
        int connectionId;
        int channelId;

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;
        byte error;

        NetworkEventType type = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, BYTE_SIZE, out dataSize, out error);
        switch (type)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log($"User {connectionId} has connected!");
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log($"User {connectionId} has disconnected!");
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
        switch ((NetworkOperationCode)msg.OperationCode)
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
                if (menuMode == MenuMode.Selected)
                    snapshotHandler.GetNeighbour(tiltMsg.IsLeft, selectedObject);
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
                spatialHandler.HandleRotation(rotationmessage.RotationRadiansDelta, selectedObject);
                break;
            case NetworkOperationCode.MenuMode:
                var modeMessage = (ModeMessage)msg;
                HandleModeChange(menuMode, (MenuMode)modeMessage.Mode);
                menuMode = (MenuMode)modeMessage.Mode;
                break;
            /*case NetworkOperationCode.Text:
                var textMsg = (TextMessage)msg;
                break;*/
            default:
                Debug.LogWarning($"Received unhandled NetworkOperationCode: {msg.OperationCode}");
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


        var hasDeleted = snapshotHandler.DeleteSnapshotsIfExist(selectedObject, shakeCount);
        if (!hasDeleted && shakeCount > 1)
        {
            analysis.ResetModel();
        }

        HandleModeChange(menuMode, MenuMode.None);
        SendClient(new ModeMessage(MenuMode.None));
    }

    private void HandleTab(TabType type)
    {
        switch(type)
        {
            case TabType.Single:
                break;
            case TabType.Double:
                if (menuMode == MenuMode.Selection && highlightedObject != null)
                {
                    selectedObject = highlightedObject;
                    if (selectedObject.TryGetComponent(out Selectable select))
                    {
                        select.SetToSelected();
                    }

                    Destroy(ray);
                    highlightedObject = null;

                    if (selectedObject.TryGetComponent(out Snapshot snap))
                    {
                        snap.SetSelected(true);
                    }

                    SendClient(new ModeMessage(MenuMode.Selected));
                }
                else if (menuMode == MenuMode.Analysis)
                {
                    slicer.TriggerSlicing();
                }
                break;
            case TabType.HoldStart:
                spatialHandler.StartMapping(selectedObject);
                break;
            case TabType.HoldEnd:
                spatialHandler.StopMapping(selectedObject);
                break;
        }
    }

    private void HandleSwipe(bool isSwipeInward, float endX, float endY, float angle)
    {
        if (isSwipeInward)
        {
            return;
        }

        if (menuMode == MenuMode.Analysis)
        {
            snapshotHandler.HandleSnapshotCreation(angle);
        }
    }

    /// <summary>
    /// Depending on mode, the scale input is used for resizing object or recognised as a grab gesture
    /// </summary>
    private void HandleScaling(float scaleMultiplier)
    {
        if(menuMode == MenuMode.Selected)
        {
            selectedObject.transform.localScale *= scaleMultiplier;
        }
        else if (selectedObject == null)
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
                    slicer.ActivateTemporaryCuttingPlane(false);
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
                isSnapshotSelected = snapshotHandler.IsSnapshot(selectedObject);
                break;
            case MenuMode.Analysis:
                ui.SetHUD(StringConstants.ExplorationModeInfo);
                ui.SetCenterText(StringConstants.ExplorationModeInfo);
                slicer.ActivateTemporaryCuttingPlane(true);
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

        if (highlightedObject != null || selectedObject != null)
        {
            UnselectObject();
            snapshotHandler.CleanUpNeighbours();
            snapshotHandler.DeactivateAllSnapshots();
        }
    }

    private void UnselectObject()
    {
        var activeObject = highlightedObject ?? selectedObject;
        Selectable selectable = activeObject.GetComponent<Selectable>();
        if (selectable)
        {
            selectable.SetToDefault();
            selectedObject = null;
            highlightedObject = null;
        }

        if (activeObject.TryGetComponent(out Snapshot snap))
        {
            snap.SetSelected(false);
        }
        GameObject.Find(StringConstants.Main).GetComponent<MeshRenderer>().material.mainTexture = null;
    }

    public void ChangeSelectedObject(GameObject newObject)
    {
        UnselectObject();
        selectedObject = newObject;
    }
    #endregion //input handling
}
