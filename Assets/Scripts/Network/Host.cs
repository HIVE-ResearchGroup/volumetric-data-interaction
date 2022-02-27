﻿using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// </summary>
public class Host : ConnectionManager
{
    private MenuMode MenuMode;

    public GameObject testGameObject;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
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

        NetworkTransport.Connect(hostId, ConfigurationConstants.DEFAULT_IP, ConfigurationConstants.DEFAULT_CONNECTING_PORT, 0, out error);
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
                Debug.Log($"Tab detected: {tabMsg.TabType}");
                // TODO - react to different tabtypes
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
                testGameObject.transform.localScale *= scaleMessage.ScaleMultiplier;
                // TODO - react to scaling or grab depending on mode
                break;
            case NetworkOperationCode.Rotation:
                var rotationmessage = (RotationMessage)msg;
                testGameObject.transform.Rotate(0.0f, 0.0f, rotationmessage.RotationRadiansDelta * Mathf.Rad2Deg);
                // TODO - react to rotation
                break;
            case NetworkOperationCode.MenuMode:
                var modeMessage = (ModeMessage)msg;
                Debug.Log("mode: " + modeMessage.Mode);
                MenuMode = (MenuMode)modeMessage.Mode;
                break;
            case NetworkOperationCode.Text:
                var textMsg = (TextMessage)msg;
                Debug.Log("Debug: " + textMsg.Text);
                break;
        }
    }
}