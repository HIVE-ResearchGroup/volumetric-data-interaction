using System.Collections.Generic;
using System.Linq;
using Constants;
using Extensions;
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
        public static SnapshotManager Instance { get; private set; }
        
        private const float SnapshotTimeThreshold = 1.0f;
        private const float CenteringRotation = -90.0f;
        
        [SerializeField]
        private GameObject tracker;

        [SerializeField]
        private TabletOverlay tabletOverlay;

        [SerializeField]
        private GameObject trackedCamera;

        [SerializeField]
        private GameObject snapshotPrefab;
        
        [SerializeField]
        private GameObject originPlanePrefab;
        
        [SerializeField]
        private Texture2D invalidTexture;

        private Timer _snapshotTimer;

        public TabletOverlay TabletOverlay => tabletOverlay;

        private List<Snapshot> Snapshots { get; } = new();
        
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
            // means downward swipe - no placement
            // TODO what?
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
            var newPosition = currPos + Quaternion.AngleAxis(angle + currRot.eulerAngles.y + CenteringRotation, Vector3.up) * Vector3.back * ConfigurationConstants.SNAPSHOT_DISTANCE;
            
            var snapshot = Instantiate(snapshotPrefab).GetComponent<Snapshot>();
            snapshot.tag = Tags.Snapshot;
            snapshot.transform.position = newPosition;
            snapshot.SetIntersectionChild(slicePlane.CalculateIntersectionPlane(), slicePlane.SlicePlaneCoordinates.StartPoint, model);
            snapshot.PlaneCoordinates = slicePlane.SlicePlaneCoordinates;

            var originPlane = Instantiate(originPlanePrefab, tabletOverlay.Main.transform.position, tabletOverlay.Main.transform.rotation);
            originPlane.transform.SetParent(model.transform);

            snapshot.Viewer = trackedCamera;
            snapshot.OriginPlane = originPlane;
            snapshot.Selected = false;
            
            Snapshots.Add(snapshot);
        }

        public void ToggleSnapshotAlignment()
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
        
        public void CreateNeighbour(bool isLeft, GameObject selectedObject)
        {
            if (!selectedObject.IsSnapshot())
            {
                return;
            }
       
            var selectedSnapshot = selectedObject.GetComponent<Snapshot>();
            var originalPlaneCoordinates = selectedSnapshot.PlaneCoordinates;
            var model = ModelManager.Instance.CurrentModel;
            
            var slicePlane = SlicePlane.Create(model, originalPlaneCoordinates);
            if (slicePlane == null)
            {
                return;
            }

            var neighbour = CreateNeighbour();
            var intersectionPlane = slicePlane.CalculateNeighbourIntersectionPlane(isLeft);
            var texture = intersectionPlane != null ? intersectionPlane.Texture : invalidTexture;
            var startPoint = intersectionPlane?.StartPoint ?? slicePlane.SlicePlaneCoordinates.StartPoint;

            var newOriginPlanePosition = GetNewOriginPlanePosition(originalPlaneCoordinates.StartPoint, startPoint, model, selectedSnapshot.OriginPlane);
                    
            neighbour.InstantiateForGo(selectedSnapshot, newOriginPlanePosition);
            neighbour.SnapshotTexture = texture;

            if (originalPlaneCoordinates.StartPoint != startPoint)
            {
                var neighbourPlane = new SlicePlaneCoordinates(originalPlaneCoordinates, startPoint);
                neighbour.PlaneCoordinates = neighbourPlane;
            }
            else
            {
                Debug.Log("No more neighbour in this direction");
            }

            Host.Instance.Selected = neighbour.gameObject;

            neighbour.SetIntersectionChild(texture, startPoint, model);
            neighbour.Selected = true;
            neighbour.gameObject.SetActive(false);
        }
        
        private Snapshot CreateNeighbour()
        {
            var neighbour = Instantiate(snapshotPrefab);
            neighbour.tag = Tags.SnapshotNeighbour;
            neighbour.GetComponent<Selectable>().enabled = false;
            return neighbour.GetComponent<Snapshot>();
        }
        
        public void CleanUpNeighbours() => Snapshots
            .Where(s => s.gameObject.IsNeighbour())
            .ForEach(s => Destroy(s.gameObject));

        public void DeactivateAllSnapshots() => Snapshots.ForEach(s => s.Selected = false);

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

        public void DeleteSnapshot(Snapshot s)
        {
            var result = Snapshots.Remove(s);
            if (!result)
            {
                Debug.LogWarning($"Tried to remove untracked Snapshot!");
            }
            s.Selected = false;
            Destroy(s.gameObject);
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
            for (var i = 0; i < Snapshots.Count && i < TabletOverlay.AdditionCount; i++)
            {
                Snapshots[i].AttachToTransform(tabletOverlay.Additions[i]);
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