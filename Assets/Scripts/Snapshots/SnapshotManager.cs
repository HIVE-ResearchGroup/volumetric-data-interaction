using System.Collections.Generic;
using System.Linq;
using Constants;
using Helper;
using JetBrains.Annotations;
using Model;
using Networking;
using Selection;
using Slicing;
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
        
        public static SnapshotManager Instance { get; private set; }
        
        private const float SnapshotTimeThreshold = 1.0f;
        private const float CenteringRotation = -90.0f;

        [SerializeField]
        private InterfaceController interfaceController;
        
        [SerializeField]
        private GameObject tracker;

        [SerializeField]
        private GameObject trackedCamera;

        [SerializeField]
        private GameObject snapshotPrefab;

        [SerializeField]
        private GameObject snapshotNeighbourPrefab;
        
        [SerializeField]
        private GameObject originPlanePrefab;
        
        [SerializeField]
        private Texture2D invalidTexture;

        private Timer _snapshotTimer;

        public InterfaceController InterfaceController => interfaceController;
        
        private List<Snapshot> Snapshots { get; } = new();

        private List<Snapshot> Neighbours { get; } = new();
        
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

        public void CreateSnapshot(float angle)
        {
            if (!_snapshotTimer.IsTimerElapsed)
            {
                return;
            }
            _snapshotTimer.StartTimerSeconds(SnapshotTimeThreshold);
            
            var model = ModelManager.Instance.CurrentModel;
            var slicePlane = model.GenerateSlicePlane();
            if (slicePlane == null)
            {
                Debug.LogWarning("SlicePlane couldn't be created!");
                return;
            }
            
            var currPos = tracker.transform.position;
            var currRot = tracker.transform.rotation;
            var newPosition = currPos + Quaternion.AngleAxis(angle + currRot.eulerAngles.y + CenteringRotation, Vector3.up) * Vector3.back * SnapshotDistance;
            
            var snapshot = Instantiate(snapshotPrefab).GetComponent<Snapshot>();
            snapshot.tag = Tags.Snapshot;
            snapshot.transform.position = newPosition;
            snapshot.SetIntersectionChild(slicePlane.CalculateIntersectionPlane(), slicePlane.SlicePlaneCoordinates.StartPoint, model);
            snapshot.PlaneCoordinates = slicePlane.SlicePlaneCoordinates;

            var mainTransform = interfaceController.Main.transform;
            var originPlane = Instantiate(originPlanePrefab, mainTransform.position, mainTransform.rotation);
            originPlane.transform.SetParent(model.transform);
            originPlane.SetActive(false);

            snapshot.Viewer = trackedCamera;
            snapshot.OriginPlane = originPlane;
            snapshot.Selectable.IsSelected = false;
            
            Snapshots.Add(snapshot);
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
        
        public void CreateNeighbour(Snapshot snapshot, NeighbourDirection direction)
        {
            var originalPlaneCoordinates = snapshot.PlaneCoordinates;
            var model = ModelManager.Instance.CurrentModel;
            
            var slicePlane = SlicePlane.Create(model, originalPlaneCoordinates);
            if (slicePlane == null)
            {
                return;
            }

            var neighbourGo = Instantiate(snapshotNeighbourPrefab);
            neighbourGo.GetComponent<Selectable>().enabled = false;
            var neighbour = neighbourGo.GetComponent<Snapshot>();
            
            var intersectionPlane = slicePlane.CalculateNeighbourIntersectionPlane(direction);
            var texture = intersectionPlane != null ? intersectionPlane.Texture : invalidTexture;
            var startPoint = intersectionPlane?.StartPoint ?? slicePlane.SlicePlaneCoordinates.StartPoint;

            var newOriginPlanePosition = GetNewOriginPlanePosition(originalPlaneCoordinates.StartPoint, startPoint, model, snapshot.OriginPlane);
                    
            neighbour.CopyFrom(snapshot);
            neighbour.OriginPlane.transform.position = newOriginPlanePosition;
            neighbour.SnapshotTexture = texture;

            if (originalPlaneCoordinates.StartPoint != startPoint)
            {
                neighbour.PlaneCoordinates = new SlicePlaneCoordinates(originalPlaneCoordinates, startPoint);
            }
            else
            {
                Debug.Log("No more neighbour in this direction");
            }

            Host.Instance.Selected = neighbour.Selectable;

            neighbour.SetIntersectionChild(texture, startPoint, model);
            neighbour.Selectable.IsSelected = true;
            neighbour.gameObject.SetActive(false);
            Neighbours.Add(neighbour);
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
                DeleteSnapshot(Snapshots[0]);
            }

            return true;
        }
        
        public void DeleteAllNeighbours()
        {
            while (Neighbours.Count > 0)
            {
                Destroy(Neighbours[0].gameObject);
                Neighbours.Remove(Neighbours[0]);
            }
        }

        public void DeleteSnapshot([NotNull] Snapshot s)
        {
            var result = Snapshots.Remove(s);
            if (!result)
            {
                Debug.LogWarning($"Trying to remove untracked Snapshot!");
            }
            s.Selectable.IsSelected = false;
            Destroy(s.gameObject);
        }

        public void ResetState()
        {
            DeleteAllSnapshots();
            DeleteAllNeighbours();
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

        private static Vector3 GetNewOriginPlanePosition(Vector3 originalStartPoint, Vector3 newStartPoint, Model.Model model, GameObject originalOriginPlane)
        {
            var direction = originalStartPoint - newStartPoint;
            var boxColliderSize = model.GetComponent<BoxCollider>().size;
            var scale = model.transform.localScale; // times scale
            var gameDimensionKey = new Vector3(boxColliderSize.z / model.XCount, boxColliderSize.y / model.YCount, boxColliderSize.x / model.ZCount);

            var offset = new Vector3(gameDimensionKey.x * direction.x * scale.x, gameDimensionKey.y * direction.y, gameDimensionKey.z * direction.z);
            return originalOriginPlane.transform.position + offset;
        }
    }
}