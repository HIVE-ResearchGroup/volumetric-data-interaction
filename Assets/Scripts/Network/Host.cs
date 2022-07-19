using Assets.Scripts.Exploration;
using System;
using System.Collections;
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
    public UnityEngine.UI.Text HUD;

    private MenuMode MenuMode;
    private GameObject ray;
    private GameObject overlayScreen;

    private Exploration analysis;

    private float snapshotTimer = 0.0f;
    private float snapshotThreshold = 3.0f;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
        overlayScreen = GameObject.Find(StringConstants.OverlayScreen);
        analysis = new Exploration(Tracker);
    }

    void Update()
    {
        UpdateMessagePump();

        if (snapshotTimer <= snapshotThreshold)
        {
            snapshotTimer += Time.deltaTime;
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
                HandleShakes(shakeMsg.Count);
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
                HandleSwipe(swipeMsg.IsInwardSwipe, swipeMsg.EndPointX, swipeMsg.EndPointY, (float)swipeMsg.Angle);
                break;
            case NetworkOperationCode.Scale:
                var scaleMessage = (ScaleMessage)msg;
                HandleScaling(scaleMessage.ScaleMultiplier);
                break;
            case NetworkOperationCode.Rotation:
                var rotationmessage = (RotationMessage)msg;
                HandleRotation(rotationmessage.RotationRadiansDelta);
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
        if (shakeCount <= 1) // one shake can happen unintentionally
        {
            return;
        }

        if (SelectedObject && SelectedObject.name.Contains(StringConstants.Snapshot))
        {
            analysis.DeleteSnapshot(SelectedObject);
            SelectedObject = null;
        }
        else if (!SelectedObject && analysis.HasSnapshots())
        {
            analysis.DeleteAllSnapshots();
        }
        else
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
                StartCoroutine(StringConstants.MapObject);
                break;
            case TabType.HoldEnd:
                StopCoroutine(StringConstants.MapObject);
                SelectedObject.GetComponent<Selectable>()?.Freeze();
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
            if (angle > 0 || snapshotThreshold > snapshotTimer) // means downward swipe - no placement
            {
                return;
            }

            snapshotTimer = 0f;
            var currPos = Tracker.transform.position;
            var currRot = Tracker.transform.rotation;
            var centeringRotation = -90;

            var newPosition = currPos + Quaternion.AngleAxis(angle + currRot.eulerAngles.y + centeringRotation, Vector3.up) * Vector3.back * ConfigurationConstants.SNAPSHOT_DISTANCE;
            analysis.PlaceSnapshot(newPosition);
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
        else if (SelectedObject == null && snapshotTimer >= snapshotThreshold)
        {
            Debug.Log("Grabbing to align snapshots");
            analysis.AlignOrMisAlignSnapshots();
            snapshotTimer = 0.0f;
        }
    }

    /// <summary>
    /// Execute rotation depending on tracker orientation and position to object
    /// The position of the object is slightly extended to check which axis is closer to the tracker
    /// Problems could occur if the object has already been rotated
    /// If so, the axis do not align as they should
    /// </summary>
    private void HandleRotation(float rotation)
    {
        if (!SelectedObject)
        {
            return;
        }
                
        var trackerTransform = Tracker.transform;
        var threshold = 20.0f;
        var downAngle = 90.0f;

        if (trackerTransform.eulerAngles.x <= downAngle + threshold && trackerTransform.eulerAngles.x >= downAngle - threshold)
        {
            SelectedObject.transform.Rotate(0.0f, rotation * Mathf.Rad2Deg, 0.0f);
            return;
        }

        var objectTransform = SelectedObject.transform;

        var distanceX = GetMinAxisDistance(1, 0, 0);
        var distanceY = GetMinAxisDistance(0, 1, 0);
        var distanceZ = GetMinAxisDistance(0, 0, 1);

        //Debug.Log(distanceX + " - " + distanceY + " - " + distanceZ);
        if (distanceX <= distanceY && distanceX <= distanceZ)
        {
            SelectedObject.transform.Rotate(rotation * Mathf.Rad2Deg, 0.0f, 0.0f);
        }
        else if (distanceY <= distanceX && distanceY <= distanceZ)
        {
            SelectedObject.transform.Rotate(0.0f, rotation * Mathf.Rad2Deg, 0.0f);
        }
        else
        {
            SelectedObject.transform.Rotate(0.0f, 0.0f, rotation * Mathf.Rad2Deg);

        }
    }

    private float GetMinAxisDistance(float x, float y, float z)
    {
        var trackerTransform = Tracker.transform;
        var objectTransform = SelectedObject.transform;

        var extendedObjectPos = objectTransform.position + new Vector3(x, y, z);
        var extendedObjectNeg = objectTransform.position + new Vector3(-x, -y, -z);

        var distancePos = Vector3.Distance(trackerTransform.position, extendedObjectPos);
        var distanceNeg = Vector3.Distance(trackerTransform.position, extendedObjectNeg);

        return distanceNeg <= distancePos ? distanceNeg : distancePos;
    }

    private void HandleModeChange(MenuMode prevMode, MenuMode currMode)
    {
        if (prevMode == currMode)
        {
            return;
        }

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
                
                HUD.text = "Tap left for 'SELECTION' and right for 'EXPLORATION'";
                break;
            case MenuMode.Selection:
                HUD.text = "SELECTION MODE";
                var overlayScreen = GameObject.Find(StringConstants.Main);
                var rayPrefab = Resources.Load(StringConstants.PrefabRay, typeof(GameObject)) as GameObject;
                ray = Instantiate(rayPrefab, overlayScreen.transform);
                break;
            case MenuMode.Analysis:
                HUD.text = "EXPLORATION MODE";
                Slicer.SetActive(true);
                break;
        }
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
            var activeObject = HighlightedObject ?? SelectedObject;
            Selectable selectable = activeObject.GetComponent<Selectable>();
            if (selectable)
            {
                selectable.SetToDefault();
                SelectedObject = null;
                HighlightedObject = null;
            }

            activeObject.GetComponent<Snapshot>()?.SetSelected(false);
        }
    }

    private IEnumerator MapObject()
    {
        float currX, currY, currZ, currRotationX, currRotationY, currRotationZ;

        var prevX = Tracker.transform.position.x;
        var prevY = Tracker.transform.position.y;
        var prevZ = Tracker.transform.position.z;

        var prevRotationX = Tracker.transform.rotation.x;
        var prevRotationY = Tracker.transform.rotation.y;
        var prevRotationZ = Tracker.transform.rotation.z;

        while (true)
        {
            currX = Tracker.transform.position.x;
            currY = Tracker.transform.position.y;
            currZ = Tracker.transform.position.z;

            currRotationX = Tracker.transform.rotation.x;
            currRotationY = Tracker.transform.rotation.y;
            currRotationZ = Tracker.transform.rotation.z;

            SelectedObject.transform.position += new Vector3(currX - prevX, currY - prevY, currZ - prevZ);
            SelectedObject.transform.rotation = new Quaternion(SelectedObject.transform.rotation.x + currRotationX - prevRotationX,
                                                               SelectedObject.transform.rotation.y + currRotationY - prevRotationY,
                                                               SelectedObject.transform.rotation.z + currRotationZ - prevRotationZ, 0.0f);

            prevX = Tracker.transform.position.x;
            prevY = Tracker.transform.position.y;
            prevZ = Tracker.transform.position.z;

            prevRotationX = currRotationX;
            prevRotationY = currRotationY;
            prevRotationZ = currRotationZ;

            yield return null;
        }
    }
    #endregion //input handling
}
