using Assets.Scripts.Exploration;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Message;
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
    [SerializeField]
    private GameObject main;
    [SerializeField]
    private MeshRenderer mainRenderer;
    [SerializeField]
    private Exploration analysis;
    [SerializeField]
    private InterfaceVisualisation ui;
    [SerializeField]
    private SpatialInteraction spatialHandler;
    [SerializeField]
    private SnapshotInteraction snapshotHandler;

    [SerializeField]
    private GameObject ray;

    [SerializeField]
    private string ip = ConfigurationConstants.DEFAULT_IP;
    [SerializeField]
    private int port = ConfigurationConstants.DEFAULT_PORT;

    private MenuMode menuMode;

    private readonly Slicer slicer;
    private GameObject selected;

    public GameObject Highlighted { get; set; }

    private void Start()
    {
        ray.SetActive(false);
        DontDestroyOnLoad(gameObject);
        Init(ConnectionType.Host);
        Connect(ip, port);
    }

    void Update()
    {
        UpdateMessagePump();
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
                Debug.Log($"Data Event by connection: {connectionId}\nMessage: {msg}");
                OnData(connectionId, channelId, recHostId, msg);
                break;
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Broadcast - save Hawaii");
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
                    snapshotHandler.GetNeighbour(tiltMsg.IsLeft, selected);
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
                spatialHandler.HandleRotation(rotationmessage.RotationRadiansDelta, selected);
                break;
            case NetworkOperationCode.MenuMode:
                var modeMessage = (ModeMessage)msg;
                HandleModeChange(menuMode, (MenuMode)modeMessage.Mode);
                menuMode = (MenuMode)modeMessage.Mode;
                break;
            case NetworkOperationCode.Text:
                var textMsg = (TextMessage)msg;
                Debug.Log($"Text: {textMsg.Text}");
                break;
            default:
                Debug.LogWarning($"Received unhandled NetworkOperationCode: {msg.OperationCode}");
                break;
        }
    }

    #region Input Handling
    private void HandleShakes(int shakeCount)
    {
        if (shakeCount < 1) // one shake can happen unintentionally
        {
            return;
        }


        var hasDeleted = snapshotHandler.DeleteSnapshotsIfExist(selected.GetComponent<Snapshot>(), shakeCount);
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
                if (menuMode == MenuMode.Selection && Highlighted != null)
                {
                    selected = Highlighted;
                    if (selected.TryGetComponent(out Selectable select))
                    {
                        select.SetToSelected();
                    }

                    ray.SetActive(false);
                    Highlighted = null;

                    if (selected.TryGetComponent(out Snapshot snap))
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
                spatialHandler.StartMapping(selected);
                break;
            case TabType.HoldEnd:
                spatialHandler.StopMapping(selected);
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
            selected.transform.localScale *= scaleMultiplier;
        }
        else if (selected == null)
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
                ray.SetActive(true);
                break;
            case MenuMode.Selected:
                isSnapshotSelected = snapshotHandler.IsSnapshot(selected);
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
        ray.SetActive(false);

        if (Highlighted != null || selected != null)
        {
            UnselectObject();
            snapshotHandler.CleanUpNeighbours();
            snapshotHandler.DeactivateAllSnapshots();
        }
    }

    private void UnselectObject()
    {
        var activeObject = Highlighted ?? selected;
        Selectable selectable = activeObject.GetComponent<Selectable>();
        if (selectable)
        {
            selectable.SetToDefault();
            selected = null;
            Highlighted = null;
        }

        if (activeObject.TryGetComponent(out Snapshot snap))
        {
            snap.SetSelected(false);
        }
        mainRenderer.material.mainTexture = null;
    }

    public void ChangeSelectedObject(GameObject newObject)
    {
        UnselectObject();
        selected = newObject;
    }
    #endregion //input handling
}
