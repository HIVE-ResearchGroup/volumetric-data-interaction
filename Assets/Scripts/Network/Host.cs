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

    private MenuMode MenuMode;
    private GameObject ray;
    private GameObject overlayScreen;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
        overlayScreen = GameObject.Find(StringConstants.OverlayScreen);
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
                HandleTab((TabType)tabMsg.TabType);
                break;
            case NetworkOperationCode.Swipe:
                var swipeMsg = (SwipeMessage)msg;

                if (!swipeMsg.IsInwardSwipe && MenuMode == MenuMode.Analysis)
                {
                    // TODO - place snapshot
                    Debug.Log("place snapshot - to be done");
                }
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

    #region Input Handling
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
                    var greenMaterial = Resources.Load(StringConstants.MateriaGreen, typeof(Material)) as Material;
                    SelectedObject.GetComponent<MeshRenderer>().material = greenMaterial;

                    Destroy(ray);
                    HighlightedObject = null;
                    SendClient(new ModeMessage(MenuMode.Selected));
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

    /// <summary>
    /// Depending on mode, the scale input is used for resizing object or recognised as a grab gesture
    /// </summary>
    private void HandleScaling(float scaleMultiplier)
    {
        if(MenuMode == MenuMode.Selected)
        {
            SelectedObject.transform.localScale *= scaleMultiplier;
        }
        // TODO recognise grab gesture!
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

        if (currMode == MenuMode.Selection)
        {
            Debug.Log("Selection started");
            var overlayScreen = GameObject.Find(StringConstants.OverlayScreen);
            var rayPrefab = Resources.Load(StringConstants.PrefabRay, typeof(GameObject)) as GameObject;
            ray = Instantiate(rayPrefab, overlayScreen.transform);
        }

        if (currMode == MenuMode.None)
        {
            // cancel selection
            if (ray)
            {
                Destroy(ray);
                ray = null;
            }

            Selectable selectable = HighlightedObject?.GetComponent<Selectable>() ?? SelectedObject?.GetComponent<Selectable>();
            if (selectable)
            {
                selectable.SetToDefault();
                SelectedObject = null;
                HighlightedObject = null;
            }
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
