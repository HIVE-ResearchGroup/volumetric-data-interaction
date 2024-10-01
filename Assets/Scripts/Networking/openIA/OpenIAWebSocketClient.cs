#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Model;
using Networking.Tablet;
using Selection;
using Snapshots;
using UnityEngine;

namespace Networking.openIA
{
    public class OpenIAWebSocketClient : MonoBehaviour
    {
        public static OpenIAWebSocketClient Instance { get; private set; } = null!;

        [SerializeField]
        private bool isOnline = true;
        
        [SerializeField]
        private bool https;
        
        [SerializeField]
        private string ip = "127.0.0.1";

        [SerializeField]
        private int port = Ports.OpenIAPort;

        [SerializeField]
        private string path = "/";

        [SerializeField]
        private float tickRate = 2.0f;

        [SerializeField]
        private GameObject viewerPrefab = null!;

        private WebSocketClient ws = null!;

        private Transform cameraTransform = null!;

        private ICommandInterpreter interpreter = null!;

        private ICommandSender sender = null!;

        private Selectable? selected;

        public bool IsOnline => isOnline;

        private Vector3 previousCameraPosition;
        
        private Quaternion previousCameraRotation;

        public ulong? ClientID { get; set; }

        public List<Viewer> Viewers { get; } = new();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnEnable()
        {
            if (!isOnline)
            {
                return;
            }

            TabletServer.Instance.MappingStarted += MappingStarted;
            TabletServer.Instance.MappingStopped += MappingStopped;
            TabletServer.Instance.Sliced += Sliced;
            TabletServer.Instance.SnapshotRemoved += SnapshotRemoved;
        }

        private async void Start()
        {
            if (!isOnline)
            {
                return;
            }
            cameraTransform = ViewModeSetter.Instance.Camera.transform;
            ws = new WebSocketClient($"{(https ? "wss" : "ws")}://{ip}:{port}{(path.StartsWith("/") ? path : "/" + path)}", HandleBinaryData, HandleText);

            var newInterpreter = new InterpreterV1(ws);
            interpreter = newInterpreter;
            sender = newInterpreter;
            
            Debug.Log("Starting WebSocket client");
            try
            {
                await ws.ConnectAsync();
            }
            catch
            {
                Debug.LogError("Couldn't connect to WebSocket Server! Check ip and path!");
                return;
            }
            Debug.Log("Connected WebSocket client");
            var runTask = ws.Run();
            var periodicCameraSender = PeriodicCameraSender();
            await newInterpreter.Start();
            await Task.WhenAll(runTask, periodicCameraSender);
            Debug.Log("WebSocket client stopped");
        }

        private void OnDisable()
        {
            if (!isOnline)
            {
                return;
            }

            TabletServer.Instance.MappingStarted -= MappingStarted;
            TabletServer.Instance.MappingStopped -= MappingStopped;
            TabletServer.Instance.Sliced -= Sliced;
            TabletServer.Instance.SnapshotRemoved -= SnapshotRemoved;
        }

        private void OnDestroy()
        {
            if (!isOnline)
            {
                return;
            }
            
            ws.Dispose();
        }

        public Viewer CreateViewer(ulong id)
        {
            var viewer = Instantiate(viewerPrefab, transform).GetComponent<Viewer>();
            viewer.ID = id;
            Viewers.Add(viewer);
            return viewer;
        }

        private async Task PeriodicCameraSender(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                cameraTransform.GetPositionAndRotation(out var position, out var rotation);
                await SendCameraPosition(position, rotation);
                var tickDelay = 1000.0f / tickRate;
                var timeBetweenTicks = TimeSpan.FromMilliseconds(tickDelay);
                await Task.Delay(timeBetweenTicks, token);
            }
        }

        private async Task SendCameraPosition(Vector3 position, Quaternion rotation)
        {
            if (!ClientID.HasValue)
            {
                return;
            }
            var model = ModelManager.Instance.CurrentModel;
            if (previousCameraPosition != position)
            {
                previousCameraPosition = position;
                var openIAPosition = CoordinateConverter.UnityToOpenIA(model, position);
                await Send(new SetObjectTranslation(ClientID.Value, openIAPosition));
            }
            if (previousCameraRotation != rotation)
            {
                previousCameraRotation = rotation;
                var localNormal = model.transform.InverseTransformDirection(rotation * Vector3.back);
                var localUp = model.transform.InverseTransformDirection(rotation * Vector3.up);
                var openIANormal = CoordinateConverter.UnityToOpenIADirection(localNormal);
                var openIAUp = CoordinateConverter.UnityToOpenIADirection(localUp);
                await Send(new SetObjectRotationNormal(ClientID.Value, openIANormal, openIAUp));
            }
        }

        private async Task Send(ICommand cmd) => await sender.Send(cmd);
        
        private Task HandleText(string text)
        {
            Debug.Log($"WS text received: \"{text}\"");
            return Task.CompletedTask;
        }
        
        private async Task HandleBinaryData(byte[] data)
        {
            Debug.Log($"WS bytes received: {BitConverter.ToString(data)}");
            await interpreter.Interpret(data);
        }
        
        private void MappingStarted(Selectable sel)
        {
            selected = sel;
        }

        private async void MappingStopped()
        {
            if (selected == null)
            {
                return;
            }

            if (!selected.TryGetComponent<Model.Model>(out var model))
            {
                return;
            }

            model.transform.GetPositionAndRotation(out var position, out var rotation);
            var openIAPosition = CoordinateConverter.UnityToOpenIA(model, position);
            await Send(new SetObjectTranslation(model.ID, openIAPosition));
            await Send(new SetObjectRotationQuaternion(model.ID, rotation));
        }

        private async void Sliced(Transform slicerTransform)
        {
            var model = ModelManager.Instance.CurrentModel;
            slicerTransform.GetPositionAndRotation(out var position, out var rotation);
            var localNormal = model.transform.InverseTransformDirection(rotation * Vector3.back);
            var openIAPosition = CoordinateConverter.UnityToOpenIA(model, position);
            var openIANormal = CoordinateConverter.UnityToOpenIADirection(localNormal);
            await Send(new CreateSnapshotNormalClient(openIAPosition, openIANormal));
        }

        private async void SnapshotRemoved(Snapshot s)
        {
            await Send(new RemoveSnapshot(s.ID));
        }
    }
}