#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants;
using Helper;
using Model;
using Networking;
using Networking.openIAExtension;
using Networking.openIAExtension.Commands;
using UnityEngine;

namespace Snapshots
{
    /*
     * TODO SnapshotManager needs a refactor urgently!
     * find out how the game should handle snapshots first of all
     * the Snapshot type is used everywhere and is still if it is a snapshot, refactor!
     * what is aligned? what is misaligned? one is tracked to the tablet and the other is placed around the player
     */
    [RequireComponent(typeof(Timer))]
    public class SnapshotManager : MonoBehaviour
    {
        private const int SnapshotDistance = 2;
        private const int NeighbourDistance = 5; // pixel, except for if it is along x-axis, then it is slices
        private const float SnapshotTimeThreshold = 1.0f;
        private const float CenteringRotation = -90.0f;

        public static SnapshotManager Instance { get; private set; } = null!;

        public event Action<Snapshot>? SnapshotMoved;
        
        [SerializeField]
        private InterfaceController interfaceController = null!;
        
        [SerializeField]
        private GameObject tracker = null!;

        [SerializeField]
        private GameObject trackedCamera = null!;

        [SerializeField]
        private GameObject snapshotPrefab = null!;
        
        [SerializeField]
        private GameObject originPlanePrefab = null!;

        [SerializeField]
        private GameObject sectionQuad = null!;

        [SerializeField]
        private OpenIaWebSocketClient openIaWebSocketClient = null!;
        
        private Timer _snapshotTimer = null!;

        public InterfaceController InterfaceController => interfaceController;

        private List<Snapshot> Snapshots { get; } = new();

        private bool _online;

        private ulong _offlineSnapshotID;

        private Snapshot? _preCreatedSnapshot;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                _snapshotTimer = GetComponent<Timer>();
            }
            else
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            _online = OnlineState.Instance.Online;
        }

        public Snapshot? GetSnapshot(ulong id) => Snapshots.FirstOrDefault(s => s.ID == id);
        
        public async Task CreateSnapshot(float angle)
        {
            if (!_snapshotTimer.IsTimerElapsed)
            {
                return;
            }
            _snapshotTimer.StartTimerSeconds(SnapshotTimeThreshold);
            
            // The openIA extension requires that all Snapshots are registered at the server and the server sends out the same data with an ID (the actual Snapshot).
            // So just send position and rotation to the server and wait.

            var slicerPosition = sectionQuad.transform.position;
            var slicerRotation = sectionQuad.transform.rotation;

            var currPos = tracker.transform.position;
            var currRot = tracker.transform.rotation;
            var newPosition = currPos + Quaternion.AngleAxis(angle + currRot.eulerAngles.y + CenteringRotation, Vector3.up) * Vector3.back * SnapshotDistance;

            if (_online)
            {
                var snapshot = CreateSnapshot(0, slicerPosition, slicerRotation);
                if (snapshot == null)
                {
                    return;
                }
                snapshot.transform.position = newPosition;
                _preCreatedSnapshot = snapshot;
                
                await openIaWebSocketClient.Send(new CreateSnapshot(slicerPosition, slicerRotation));
            }
            else
            {
                var snapshot = CreateSnapshot(_offlineSnapshotID++, slicerPosition, slicerRotation);
                if (snapshot == null)
                {
                    return;
                }
                snapshot.transform.position = newPosition;
            }
        }
        
        public Snapshot? CreateSnapshot(ulong id, Vector3 slicerPosition, Quaternion slicerRotation)
        {
            if (_online && _preCreatedSnapshot != null)
            {
                // If we are online, this method is called as some sort of callback where the snapshot is
                // already created and we only need to synchronize its ID.
                // Thank god for references or else we would have a problem.
                
                // There is still a race condition here:
                // When the server sends a snapshot from another client and this client simultaneously tries
                // to create a snapshot, the wrong ID might be set.
                var snapshotReference = _preCreatedSnapshot;
                _preCreatedSnapshot = null;
                snapshotReference.ID = id;
                return snapshotReference;
            }

            var snapshot = CreateSnapshot_internal(id, slicerPosition, slicerRotation);

            if (snapshot != null)
            {
                Snapshots.Add(snapshot);
            }

            return snapshot;
        }
        
        public void ToggleSnapshotsAttached()
        {
            if (!_snapshotTimer.IsTimerElapsed)
            {
                return;
            }
            _snapshotTimer.StartTimerSeconds(SnapshotTimeThreshold);

            if (AreSnapshotsAttached())
            {
                DetachSnapshots();
            }
            else
            {
                AttachSnapshots();
            }
        }

        public void Move(Snapshot snapshot, NeighbourDirection direction)
        {
            snapshot.MoveSliceZ(NeighbourDistance * (int)direction);
            SnapshotMoved?.Invoke(snapshot);
        }

        public void DeactivateAllSnapshots() => Snapshots.ForEach(s => s.Selectable.IsSelected = false);

        /// <summary>
        /// Delete all Snapshots.
        /// </summary>
        /// <returns>Returns true if any snapshots have been deleted, false if nothing happened.</returns>
        public bool DeleteAllSnapshots()
        {
            if (Snapshots.Count == 0)
            {
                return false;
            }
            
            while (Snapshots.Count > 0)
            {
                DeleteSnapshot(Snapshots.First());
            }

            return true;
        }

        public bool DeleteSnapshot(Snapshot s)
        {
            var result = Snapshots.Remove(s);
            if (!result)
            {
                Debug.LogWarning($"Trying to remove untracked Snapshot!");
                return false;
            }
            s.Selectable.IsSelected = false;
            Destroy(s.gameObject);
            return true;
        }

        public bool DeleteSnapshot(ulong id)
        {
            var snapshot = Snapshots.FirstOrDefault(s => s.ID == id);
            // ReSharper disable once InvertIf
            if (snapshot == null)
            {
                Debug.LogWarning($"Tried deleting non-existent Snapshot with ID {id}.");
                return false;
            }
            return DeleteSnapshot(snapshot);   
        }

        public void ResetState()
        {
            DeleteAllSnapshots();
        }

        private Snapshot? CreateSnapshot_internal(ulong id, Vector3 slicerPosition, Quaternion slicerRotation)
        {
            var model = ModelManager.Instance.CurrentModel;
            var slicePlane = model.GenerateSlicePlane(slicerPosition, slicerRotation);
            if (slicePlane == null)
            {
                Debug.LogWarning("SlicePlane couldn't be created!");
                return null;
            }
            
            var texture = slicePlane.CalculateIntersectionPlane();
            var snapshot = Instantiate(snapshotPrefab).GetComponent<Snapshot>();
            snapshot.ID = id;
            snapshot.tag = Tags.Snapshot;
            snapshot.SetIntersectionChild(texture, slicePlane.SlicePlaneCoordinates.StartPoint, model);
            snapshot.PlaneCoordinates = slicePlane.SlicePlaneCoordinates;
        
            var mainTransform = interfaceController.Main.transform;
            var originPlane = Instantiate(originPlanePrefab, mainTransform.position, mainTransform.rotation);
            originPlane.transform.SetParent(model.transform);
            originPlane.SetActive(false);
        
            snapshot.Viewer = trackedCamera;
            snapshot.OriginPlane = originPlane;
            snapshot.Selectable.IsSelected = false;
            
            return snapshot;
        }
        
        /// <summary>
        /// It could happen that not all snapshots are aligned due to the size restriction.
        /// </summary>
        private bool AreSnapshotsAttached() => Snapshots.Any(s => s.IsAttached);

        /// <summary>
        /// Only up to 5 snapshots can be aligned. The rest needs to stay in their original position.
        /// </summary>
        private void AttachSnapshots()
        {
            for (var i = 0; i < Snapshots.Count && i < InterfaceController.AdditionCount; i++)
            {
                Snapshots[i].AttachToTransform(interfaceController.Main.parent, interfaceController.Additions[i].position);
            }
        }
        
        private void DetachSnapshots() => Snapshots.ForEach(s => s.Detach());
    }
}