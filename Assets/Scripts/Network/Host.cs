using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Helper;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// </summary>
public class Host : ConnectionManager
{
    public GameObject Tracker;
    public GameObject SelectedObject;
    public GameObject HighlightedObject;
    public UnityEngine.UI.Text HUD;

    private MenuMode MenuMode;
    private GameObject ray;
    private GameObject overlayScreen;

    private Exploration analysis;

    private float alignTimer = 0.0f;
    private float alignThreshold = 5.0f;

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

        }

        if (alignTimer <= alignThreshold)
        {
            alignTimer += Time.deltaTime;
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
                HandleSwipe(swipeMsg.IsInwardSwipe, swipeMsg.EndPointX, swipeMsg.EndPointY, swipeMsg.Angle);
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
        // can only shake once now?
        //if (shakeCount <= 1) // one shake can happen unintentionally
        //{
        //    return;
        //}

        // if snapshot is selected - rm snapshot
        if (SelectedObject && SelectedObject.name.Contains(StringConstants.Snapshot))
        {
            analysis.DeleteSnapshot(SelectedObject);
            SelectedObject = null;
        }
        // else if snapshots exit - rm all snapshots
        else if (!SelectedObject)
        {
            analysis.DeleteAllSnapshots();
        }
        // else reset model
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
                // no need for action?
                break;
            case TabType.Double:
                if (MenuMode == MenuMode.Selection && HighlightedObject != null)
                {
                    SelectedObject = HighlightedObject;
                    SelectedObject.GetComponent<Selectable>()?.SetToSelected();

                    Destroy(ray);
                    HighlightedObject = null;

                    //check for snapshot?
                    SelectedObject.GetComponent<Snapshot>()?.SetSelected(true);

                    SendClient(new ModeMessage(MenuMode.Selected));
                }
                else if (MenuMode == MenuMode.Analysis)
                {
                    Debug.Log("Freeze cutting plane and create another one");
                    analysis.DispatchCurrentCuttingPlane();
                    analysis.CreateCuttingPlane();
                }
                break;
            case TabType.HoldStart:
                Debug.Log("Coroutine - Hold Start");
                StartCoroutine(StringConstants.MapObject);
                break;
            case TabType.HoldEnd:
                Debug.Log("Coroutine - Hold End");
                StopCoroutine(StringConstants.MapObject);
                break;
        }
    }

    private void HandleSwipe(bool isSwipeInward, float endX, float endY, double angle)
    {
        if (isSwipeInward)
        {
            return;
        }

        Debug.Log("Swipe angle: " + angle);
        if (MenuMode == MenuMode.Analysis)
        {
            var (xDistance, yDistance) = GetSnapshotPosition(angle);
            var currPos = Tracker.transform.position;
            var newPosition = new Vector3(currPos.x + xDistance, currPos.y + yDistance);

            analysis.PlaceSnapshot(newPosition);
        }
    }


    // TODO - move this part to own class after merge with cutting plane calculation
    /// <summary>
    /// Calculate sides of rectngular triangle
    /// Need to pay attention to size of angle 
    /// Angle will always be between 0 and 180 
    /// Positive angle for bottom swipe
    /// Negative angle for top swipe
    /// </summary>
    private (float x, float y) GetSnapshotPosition(double angle)
    {
        var isTopSide = angle < 0;

        if (angle == 90)
        {
            return (0, isTopSide ? ConfigurationConstants.SNAPSHOT_DISTANCE : -ConfigurationConstants.SNAPSHOT_DISTANCE);
        }

        angle = Math.Abs(angle);
        var isAngleOver90 = angle > 90;
        if (isAngleOver90)
        {
            angle = 180 - angle;
        }

        var yDistance = MathHelper.CalculateRectangularTriangle(angle, ConfigurationConstants.SNAPSHOT_DISTANCE);
        var xDistance = MathHelper.CalculatePytagoras(ConfigurationConstants.SNAPSHOT_DISTANCE, yDistance);

        if (!isTopSide)
        {
            yDistance *= -1;
        }

        if (isAngleOver90)
        {
            xDistance *= -1;
        }

        return (xDistance, yDistance);
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
        else if (SelectedObject == null && alignTimer >= alignThreshold)
        {
            Debug.Log("Grabbing to align snapshots");
            analysis.AlignSnapshots();
            alignTimer = 0.0f;
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
        var extendedObjectX = objectTransform.position + new Vector3(1.0f, 0.0f, 0.0f);
        var extendedObjectZ = objectTransform.position + new Vector3(0.0f, 0.0f, 1.0f);

        if (Vector3.Distance(trackerTransform.position, extendedObjectX) < Vector3.Distance(trackerTransform.position, extendedObjectZ))
        {
            SelectedObject.transform.Rotate(rotation * Mathf.Rad2Deg, 0.0f, 0.0f);
        }
        else
        {
            SelectedObject.transform.Rotate(0.0f, 0.0f, rotation * Mathf.Rad2Deg);
        }
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
                if (ray)
                {
                    Destroy(ray);
                    ray = null;
                }

                if (HighlightedObject != null  || SelectedObject != null)
                {
                    var activeObject = HighlightedObject ?? SelectedObject;
                    Selectable selectable = activeObject.GetComponent<Selectable>();
                    if (selectable)
                    {
                        selectable.SetToDefault();
                        SelectedObject = null;
                        HighlightedObject = null;
                    }

                    //reset snapshot if it was snapshot
                    activeObject.GetComponent<Snapshot>()?.SetSelected(false);
                }

                analysis.DeleteAllCuttingPlanes();

                HUD.text = "Tap left for 'SELECTION' and right for 'EXPLORATION'";
                break;
            case MenuMode.Selection:
                HUD.text = "SELECTION MODE";
                Debug.Log("Selection started");
                var overlayScreen = GameObject.Find(StringConstants.Main);
                var rayPrefab = Resources.Load(StringConstants.PrefabRay, typeof(GameObject)) as GameObject;
                ray = Instantiate(rayPrefab, overlayScreen.transform);
                break;
            case MenuMode.Analysis:
                    HUD.text = "EXPLORATION MODE";
                    // add empty slicer (all three) to the tracker
                    // remove piece by piece as soon as one thingy is frozen...
                    analysis.CreateCuttingPlane();
                break;
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
