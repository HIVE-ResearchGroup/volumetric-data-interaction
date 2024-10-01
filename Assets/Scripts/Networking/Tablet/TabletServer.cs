#nullable enable

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Extensions;
using Networking.Screens;
using Slicing;
using Selection;
using Snapshots;
using UnityEngine;
using Networking.openIA;

namespace Networking.Tablet
{
    public class TabletServer : MonoBehaviour
    {
        public static TabletServer Instance { get; private set; } = null!;
        
        [SerializeField]
        private GameObject ray = null!;
        
        [SerializeField]
        private Slicer slicer = null!;

        [SerializeField]
        private GameObject tablet = null!;

        [SerializeField]
        private MappingAnchor mappingAnchor = null!;
        
        [SerializeField]
        private int port = Ports.TabletPort;
        
        private TcpListener server = null!;
        private TcpClient? tabletClient;
        private NetworkStream? tabletStream;
        private Task? receivingTask;

        private Selectable? selected;
        private Selectable? highlighted;

        private Selectable? Selected
        {
            get => selected;
            set
            {
                if (selected == value)
                {
                    return;
                }

                if (selected != null)
                {
                    selected.IsSelected = false;
                }

                selected = value;

                if (selected != null)
                {
                    selected.IsSelected = true;
                }
            }
        }

        public Selectable? Highlighted
        {
            get => highlighted;
            set
            {
                if (highlighted == value)
                {
                    return;
                }

                if (highlighted != null)
                {
                    highlighted.IsHighlighted = false;
                }

                highlighted = value;

                if (highlighted != null)
                {
                    highlighted.IsHighlighted = true;
                }
            }
        }

        public event Action<Selectable>? MappingStarted;

        public event Action? MappingStopped;

        public event Action<Transform>? Sliced;

        public event Action<Snapshot>? SnapshotRemoved;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                server = new TcpListener(IPAddress.Any, port);
            }
            else
            {
                Destroy(this);
            }
        }

        private async void OnEnable()
        {
            SnapshotManager.Instance.SnapshotRemoved += OnSnapshotRemoved;

            Debug.Log($"TabletServer started on port {port}");
            server.Start();
            try
            {
                tabletClient = await server.AcceptTcpClientAsync();
            }
            catch
            {
                Debug.Log("Tablet never connected.");
                return;
            }
            Debug.Log("Client connected");
            tabletStream = tabletClient.GetStream();

            receivingTask = Run();
        }

        private void Start()
        {
            ray.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (!ray.activeInHierarchy)
            {
                return;
            }

            var tabTran = tablet.transform;
            if (Physics.Raycast(tabTran.position, tabTran.up, out var hit, 100.0f, Layers.Selectable))
            {
                if (hit.transform.gameObject.TryGetComponent<Selectable>(out var selectable))
                {
                    Highlighted = selectable;
                }
            }
            else
            {
                Highlighted = null;
            }
        }

        private async void OnDisable()
        {
            SnapshotManager.Instance.SnapshotRemoved -= OnSnapshotRemoved;

            tabletClient?.Close();
            server.Stop();
            if (receivingTask != null)
            {
                await receivingTask;
            }
        }

        private async Task Run()
        {
            var commandIdentifier = new byte[1];
            while (true)
            {
                try
                {
                    if (tabletStream == null)
                    {
                        return;
                    }
                    await tabletStream.ReadAllAsync(commandIdentifier, 0, 1);
                    Debug.Log($"Received command {commandIdentifier[0]}");
                    try
                    {
                        switch (commandIdentifier[0])
                        {
                            case Categories.Scale:
                                {
                                    var buffer = new byte[ScaleCommand.Size];
                                    buffer[0] = commandIdentifier[0];
                                    await tabletStream.ReadAllAsync(buffer, 1, sizeof(float));
                                    var scaleCommand = ScaleCommand.FromByteArray(buffer);
                                    OnScale(scaleCommand.Value);
                                    break;
                                }
                            case Categories.SelectionMode:
                                {
                                    OnSelectionMode();
                                    break;
                                }
                            case Categories.SlicingMode:
                                {
                                    OnSlicingMode();
                                    break;
                                }
                            case Categories.Select:
                                {
                                    OnSelect();
                                    break;
                                }
                            case Categories.Deselect:
                                {
                                    OnDeselect();
                                    break;
                                }
                            case Categories.Slice:
                                {
                                    OnSlice();
                                    break;
                                }
                            case Categories.RemoveSnapshot:
                                {
                                    OnRemoveSnapshot();
                                    break;
                                }
                            case Categories.ToggleAttached:
                                {
                                    OnToggleAttached();
                                    break;
                                }
                            case Categories.HoldBegin:
                                {
                                    OnHoldBegin();
                                    break;
                                }
                            case Categories.HoldEnd:
                                {
                                    OnHoldEnd();
                                    break;
                                }
                            case Categories.SendToScreen:
                                {
                                    await OnSendToScreen();
                                    break;
                                }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        private void OnScale(float value)
        {
            Debug.LogWarning("Scaling is not implemented");
        }

        private void OnSelectionMode()
        {
            slicer.SetCuttingActive(false);
            ray.SetActive(true);
        }

        private void OnSlicingMode()
        {
            slicer.SetCuttingActive(true);
            ray.SetActive(false);
        }

        private void OnSelect()
        {
            if (Highlighted == null)
            {
                return;
            }

            Selected = Highlighted;

            if (tabletStream == null)
            {
                return;
            }
            
            if (Selected.TryGetComponent<Snapshot>(out _))
            {
                tabletStream.WriteByte(Categories.SelectedSnapshot);
            }
            else
            {
                tabletStream.WriteByte(Categories.SelectedModel);
                ray.SetActive(false);
            }
        }

        private void OnDeselect()
        {
            Selected = null;
            ray.SetActive(true);
        }

        private void OnSlice()
        {
            if (OpenIAWebSocketClient.Instance.IsOnline)
            {
                Sliced?.Invoke(slicer.transform);
                return;
            }
            
            var snapshot = slicer.CreateSnapshot();
            if (snapshot != null)
            {
                Sliced?.Invoke(slicer.transform);
            }
        }

        private void OnRemoveSnapshot()
        {
            if (Selected == null)
            {
                return;
            }
            if (!Selected.TryGetComponent<Snapshot>(out var snapshot))
            {
                return;
            }

            if (SnapshotManager.Instance.DeleteSnapshot(snapshot))
            {
                tabletStream?.WriteByte(Categories.SnapshotRemoved);
                SnapshotRemoved?.Invoke(snapshot);
            }
        }

        private void OnToggleAttached()
        {
            if (Selected == null)
            {
                return;
            }
            if (!Selected.TryGetComponent<Snapshot>(out var snapshot))
            {
                return;
            }

            if (snapshot.IsAttached)
            {
                snapshot.Detach();
            }
            else
            {
                snapshot.Attach();
            }
        }

        private void DetachAllSnapshots()
        {
            SnapshotManager.Instance.DetachAllSnapshots();
        }

        private void OnHoldBegin()
        {
            if (Selected == null)
            {
                return;
            }

            mappingAnchor.StartMapping(Selected.transform);
            MappingStarted?.Invoke(Selected);
        }

        private void OnHoldEnd()
        {
            if (mappingAnchor.StopMapping())
            {
                MappingStopped?.Invoke();
            }
        }

        private async Task OnSendToScreen()
        {
            if (Selected == null)
            {
                return;
            }
            if (!Selected.TryGetComponent<Snapshot>(out var snapshot))
            {
                return;
            }

            await ScreenServer.Instance.SendAsync(tablet.transform.position, tablet.transform.up, snapshot.SnapshotTexture);
        }
        
        private void OnSnapshotRemoved(ulong id)
        {
            if (Selected == null)
            {
                return;
            }

            if (!Selected.TryGetComponent<Snapshot>(out var snapshot))
            {
                return;
            }

            if (snapshot.ID != id)
            {
                return;
            }

            tabletStream?.WriteByte(Categories.SnapshotRemoved);
        }
    }
}
