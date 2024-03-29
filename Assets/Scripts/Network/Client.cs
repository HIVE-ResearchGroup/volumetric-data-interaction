﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Firewall for Domain Network needs to be deactivated!
/// </summary>
public class Client : ConnectionManager
{
    private Menu menu;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
        menu = GameObject.Find(StringConstants.UI).GetComponent<Menu>();
    }

    public void Update()
    {
        UpdateMessagePump();
    }

    protected override void Init()
    {
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc, 1);
        hostId = NetworkTransport.AddHost(topo, 0);

        connectionId = NetworkTransport.Connect(hostId, ConfigurationConstants.HOST_IP, ConfigurationConstants.DEFAULT_CONNECTING_PORT, 0, out error);
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
                Debug.Log("Connected ");
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnected");
                break;
            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetworkMessage msg = (NetworkMessage)formatter.Deserialize(ms);

                OnData(connectionId, channelId, recHostId, msg);
                break;
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected data");
                break;
        }
    }

    private void OnData(int connectionId, int channelId, int recHostId, NetworkMessage msg)
    {
        switch (msg.OperationCode)
        {
            case NetworkOperationCode.None:
                break;            
            case NetworkOperationCode.MenuMode:
                var modeMessage = (ModeMessage)msg;

                if ((MenuMode)modeMessage.Mode == MenuMode.Selected)
                {
                    menu.SelectedObject();
                }
                break;
            case NetworkOperationCode.Text:
                var textMessage = (TextMessage)msg;
                menu.SendDebug(textMessage.Text);
                break;
        }
    }

    public void SendServer(NetworkMessage message)
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
        HandleMessageContent(message);
    }

    /// <summary>
    /// Apply input which is necessary for the client directly
    /// </summary>
    private void HandleMessageContent(NetworkMessage msg)
    {
        switch (msg.OperationCode)
        {
            case NetworkOperationCode.Swipe:
                var swipeMsg = (SwipeMessage)msg;

                if (swipeMsg.IsInwardSwipe)
                {
                    SendServer(new TextMessage("Cancel initiated from client"));
                    menu.Cancel();
                }
                break;
            case NetworkOperationCode.Tab:
                var tabMsg = (TabMessage)msg;
                if (tabMsg.TabType == (int)TabType.HoldStart)
                {
                    SendServer(new TextMessage("Hold Start initiated from client"));
                    menu.StartMapping();
                }
                else if (tabMsg.TabType == (int)TabType.HoldEnd)
                {
                    SendServer(new TextMessage("Hold End initiated from client"));
                    menu.StopMapping();
                }
                break;
        }
    }
}
