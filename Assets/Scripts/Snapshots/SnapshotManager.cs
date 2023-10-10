﻿using System.Collections.Generic;
using System.Linq;
using Constants;
using Extensions;
using Model;
using Networking;
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
    public class SnapshotManager : MonoBehaviour
    {
        public static SnapshotManager Instance { get; private set; }
        
        private const float SnapshotThreshold = 3.0f;
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

        private float _snapshotTimer;

        public TabletOverlay TabletOverlay => tabletOverlay;

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
            // means downward swipe - no placement
            // TODO what?
            if (SnapshotThreshold > _snapshotTimer)
            {
                return;
            }

            _snapshotTimer = 0f;

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

        // TODO what is this doing?
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
            neighbour.SetOverlayTexture(true);
            neighbour.Selected = true;
            neighbour.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Only up to 5 snapshots can be aligned. The rest needs to stay in their original position.
        /// </summary>
        private void AlignSnapshots(IEnumerable<Snapshot> snapshots)
        {
            /*var overlay = tracker.transform.Find(StringConstants.OverlayScreen);
            if (!overlay)
            {
                Debug.Log("Alignment not possible. Overlay screen not found as child of tracker.");
            }*/

            var snapList = snapshots.ToList();
            var cachedTabletTransform = tabletOverlay.transform;
            for (var i = 0; i < snapList.Count && i < TabletOverlay.AdditionCount; i++)
            {
                var child = tabletOverlay.Additions[i];
                snapList[i].SetAligned(cachedTabletTransform);
                snapList[i].transform.SetPositionAndRotation(child.position, new Quaternion());
                snapList[i].transform.localScale = new Vector3(1, 0.65f, 0.1f);
            }
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

        public void DeactivateAllSnapshots() => GetAllSnapshots().ForEach(s => s.Selected = false);
        
        public bool DeleteSnapshotsIfExist(Snapshot selectedObject, int shakeCounter)
        {
            if (selectedObject && selectedObject.gameObject.IsSnapshot()) {
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
        // TODO there is no prefab instanced directly! everything is marked as clone! why do we even bother?!
        private IEnumerable<Snapshot> GetAllSnapshots() => Snapshots
            .Where(s => s.gameObject.IsSnapshot() && s.gameObject.IsClone());

        private void DeleteSnapshot(Snapshot s)
        {
            if (!s.gameObject.IsClone())
            {
                return;
            }

            Snapshots.Remove(s);
            s.Selected = false;
            Destroy(s.OriginPlane);
            Destroy(s.gameObject);
        }

        /// <summary>
        /// It could happen that not all snapshots are aligned due to the size restriction.
        /// </summary>
        private static bool AreSnapshotsAligned(IEnumerable<Snapshot> snapshots) => snapshots.Any(s => !s.IsLookingAt);

        private static void MisalignSnapshots(IEnumerable<Snapshot> snapshots) => snapshots.ForEach(s => s.SetMisaligned());

        private static Vector3 GetNewOriginPlanePosition(Vector3 originalStartPoint, Vector3 newStartPoint, Model.Model model, GameObject originalOriginPlane)
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
    }
}