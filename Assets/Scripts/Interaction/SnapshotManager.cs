using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Exploration;
using Extensions;
using Networking;
using UnityEngine;

namespace Interaction
{
    public class SnapshotManager : MonoBehaviour
    {
        public static SnapshotManager Instance { get; private set; }
        
        private const float SnapshotThreshold = 3.0f;
        private const float CenteringRotation = -90.0f;
        
        [SerializeField]
        private GameObject tracker;

        [SerializeField]
        private Transform tabletOverlay;

        [SerializeField]
        private GameObject main;
        
        [SerializeField]
        private Host host;
        
        [SerializeField]
        private GameObject trackedCamera;

        [SerializeField]
        private GameObject snapshotPrefab;
        
        [SerializeField]
        private GameObject originPlanePrefab;
        
        [SerializeField]
        private Texture2D invalidTexture;

        private float _snapshotTimer;

        private List<Snapshot> Snapshots { get; } = new();
        
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
        
        private void Update()
        {
            if (_snapshotTimer <= SnapshotThreshold)
            {
                _snapshotTimer += Time.deltaTime;
            }
        }

        public void CreateSnapshot(float angle)
        {
            if (SnapshotThreshold > _snapshotTimer) // means downward swipe - no placement
            {
                return;
            }

            _snapshotTimer = 0f;
            var currPos = tracker.transform.position;
            var currRot = tracker.transform.rotation;

            var newPosition = currPos + Quaternion.AngleAxis(angle + currRot.eulerAngles.y + CenteringRotation, Vector3.up) * Vector3.back * ConfigurationConstants.SNAPSHOT_DISTANCE;
            PlaceSnapshot(newPosition);
        }

        public void ToggleSnapshotAlignment()
        {
            if (_snapshotTimer <= SnapshotThreshold)
            {
                return;
            }

            _snapshotTimer = 0f;
            var snapshots = GetAllSnapshots().ToList();

            if (AreSnapshotsAligned(snapshots))
            {
                MisalignSnapshots(snapshots);
            }
            else
            {
                AlignSnapshots(snapshots);
            }
        }
        
        public void GetNeighbour(bool isLeft, GameObject selectedObject)
        {
            if (!IsSnapshot(selectedObject))
            {
                return;
            }
       
            var selectedSnapshot = selectedObject.GetComponent<Snapshot>();
            var originalPlaneCoordinates = selectedSnapshot.PlaneCoordinates;

            var neighbourGo = CreateNeighbourGameObject();
            try
            {
                var model = ModelManager.Instance.CurrentModel;
                var slicePlane = new SlicePlane(model, originalPlaneCoordinates);
                var intersectionPlane = slicePlane.CalculateNeighbourIntersectionPlane(isLeft);
                var texture = intersectionPlane != null ? intersectionPlane.Texture : invalidTexture;
                var startPoint = intersectionPlane?.StartPoint ?? slicePlane.SlicePlaneCoordinates.StartPoint;

                var newOriginPlanePosition = GetNewOriginPlanePosition(originalPlaneCoordinates.StartPoint, startPoint, model, selectedSnapshot.OriginPlane);
                        
                var neighbourSnap = neighbourGo.GetComponent<Snapshot>();
                neighbourSnap.InstantiateForGo(selectedSnapshot, newOriginPlanePosition);
                neighbourSnap.SnapshotTexture = texture;

                if (IsNeighbourStartPointDifferent(originalPlaneCoordinates.StartPoint, startPoint))
                {
                    var neighbourPlane = new SlicePlaneCoordinates(originalPlaneCoordinates, startPoint);
                    neighbourSnap.PlaneCoordinates = neighbourPlane;
                }
                else
                {
                    Debug.Log("No more neighbour in this direction");
                }

                host.Selected = neighbourGo;

                neighbourSnap.SetIntersectionChild(texture, startPoint, model);
                neighbourSnap.SetOverlayTexture(true);
                neighbourSnap.Selected = true;
                neighbourGo.SetActive(false);
            }
            catch (Exception)
            {
                Destroy(neighbourGo);
            }
        }
        
        /// <summary>
        /// Only up to 5 snapshots can be aligned. The rest needs to stay in their original position
        /// </summary>
        private void AlignSnapshots(IEnumerable<Snapshot> snapshots)
        {
            /*var overlay = tracker.transform.Find(StringConstants.OverlayScreen);
            if (!overlay)
            {
                Debug.Log("Alignment not possible. Overlay screen not found as child of tracker.");
            }*/

            var snapList = snapshots.ToList();
            for (var i = 0; i < snapList.Count && i < 5; i++)
            {
                var child = tabletOverlay.GetChild(i + 1); // first child is main overlay
                snapList[i].SetAligned(tabletOverlay);
                snapList[i].transform.SetPositionAndRotation(child.position, new Quaternion());
                snapList[i].transform.localScale = new Vector3(1, 0.65f, 0.1f);
            }
        }

        private void PlaceSnapshot(Vector3 newPosition)
        {
            var snapshot = Instantiate(snapshotPrefab).GetComponent<Snapshot>();
            snapshot.transform.position = newPosition;

            var model = ModelManager.Instance.CurrentModel;
            try
            {
                var slicePlane = model.GetIntersectionAndTexture();
                snapshot.SetIntersectionChild(slicePlane.CalculateIntersectionPlane(), slicePlane.SlicePlaneCoordinates.StartPoint, model);
                snapshot.PlaneCoordinates = slicePlane.SlicePlaneCoordinates;
            }
            catch (Exception e)
            {
                Destroy(snapshot);
                Debug.LogError($"Error occured on snapshot creation: {e.Message}");
                return;
            }

            var originPlane = Instantiate(originPlanePrefab, main.transform.position, main.transform.rotation);
            originPlane.transform.SetParent(model.transform);

            snapshot.Viewer = trackedCamera;
            snapshot.OriginPlane = originPlane;
            snapshot.Selected = false;
            
            Snapshots.Add(snapshot);
        }
        
        private GameObject CreateNeighbourGameObject()
        {
            var neighbourGo = Instantiate(snapshotPrefab);
            //neighbourGo.name = StringConstants.Neighbour;
            neighbourGo.tag = Tags.SnapshotNeighbour;
            Destroy(neighbourGo.GetComponent<Selectable>());
            return neighbourGo;
        }
        
        public void CleanUpNeighbours() => Snapshots
            .Where(s => IsNeighbour(s.gameObject))
            .ForEach(s => Destroy(s.gameObject));

        public void DeactivateAllSnapshots() => GetAllSnapshots().ForEach(s => s.Selected = false);

        public static bool IsSnapshot(GameObject selectedObject)
        {
            if (selectedObject == null)
            {
                return false;
            }

            return selectedObject.CompareTag(Tags.Snapshot) || IsNeighbour(selectedObject);
        }
        
        public bool DeleteSnapshotsIfExist(Snapshot selectedObject, int shakeCounter)
        {
            if (selectedObject && IsSnapshot(selectedObject.gameObject)) {
                DeleteSnapshot(selectedObject);
                return true;
            }
            if (shakeCounter > 1 && !selectedObject && GetAllSnapshots().Count() > 1)
            {
                DeleteAllSnapshots();
                return true;
            }
            return false;
        }

        public void DeleteAllSnapshots()
        {
            foreach (var s in GetAllSnapshots())
            {
                DeleteSnapshot(s);
            }
        }

        // Get all snapshots without prefab
        private IEnumerable<Snapshot> GetAllSnapshots() => Snapshots
            .Where(s => IsSnapshot(s.gameObject) && IsClone(s.gameObject));

        private void DeleteSnapshot(Snapshot s)
        {
            if (!IsClone(s.gameObject))
            {
                return;
            }

            Snapshots.Remove(s);
            s.Selected = false;
            Destroy(s.OriginPlane);
            Destroy(s.gameObject);
        }

        /// <summary>
        /// It could happen that nor all snapshots are aligned due to the size restriction
        /// </summary>
        private static bool AreSnapshotsAligned(IEnumerable<Snapshot> snapshots) => snapshots.Any(s => !s.IsLookingAt);

        private static void MisalignSnapshots(IEnumerable<Snapshot> snapshots) => snapshots.ForEach(s => s.SetMisaligned());

        private static Vector3 GetNewOriginPlanePosition(Vector3 originalStartPoint, Vector3 newStartPoint, Model model, GameObject originalOriginPlane)
        {
            var direction = originalStartPoint - newStartPoint;
            var boxColliderSize = model.GetComponent<BoxCollider>().size;
            var scale = model.transform.localScale; // times scale
            var gameDimensionKey = new Vector3(boxColliderSize.z / model.XCount, boxColliderSize.y / model.YCount, boxColliderSize.x / model.ZCount);

            var offSet = new Vector3(gameDimensionKey.x * direction.x * scale.x, gameDimensionKey.y * direction.y, gameDimensionKey.z * direction.z);
            var newPosition = originalOriginPlane.transform.position;
            newPosition += offSet;
            return newPosition;
        }

        private static bool IsNeighbourStartPointDifferent(Vector3 originalStartpoint, Vector3 neighbourStartpoint) => originalStartpoint != neighbourStartpoint;

        private static bool IsNeighbour(GameObject obj) => obj.CompareTag(Tags.SnapshotNeighbour);

        private static bool IsClone(GameObject obj) => obj.name.Contains(StringConstants.Clone);
    }
}