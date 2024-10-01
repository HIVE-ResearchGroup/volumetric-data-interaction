#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Slicing;
using UnityEngine;

namespace Snapshots
{
    public class SnapshotManager : MonoBehaviour
    {
        private const int SnapshotDistance = 2;
        private const float SnapshotTimeThreshold = 1.0f;
        private const float CenteringRotation = -90.0f;

        public static SnapshotManager Instance { get; private set; } = null!;

        [SerializeField]
        private TabletOverlay tabletOverlay = null!;
        
        [SerializeField]
        private GameObject tracker = null!;

        [SerializeField]
        private GameObject snapshotPrefab = null!;
        
        [SerializeField]
        private GameObject originPlanePrefab = null!;

        [SerializeField]
        private GameObject sectionQuad = null!;

        private readonly Timer snapshotTimer = new();

        public TabletOverlay TabletOverlay => tabletOverlay;

        public event Action<ulong>? SnapshotRemoved;

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

        public Snapshot? GetSnapshot(ulong id) => Snapshots.FirstOrDefault(s => s.ID == id);
        
        public void CreateSnapshot(float angle)
        {
            if (!snapshotTimer.IsTimerElapsed)
            {
                return;
            }
            snapshotTimer.StartTimerSeconds(SnapshotTimeThreshold);
            
            // The openIA extension requires that all Snapshots are registered at the server and the server sends out the same data with an ID (the actual Snapshot).
            // So just send position and rotation to the server and wait.
            sectionQuad.transform.GetPositionAndRotation(out var slicerPosition, out var slicerRotation);
            tracker.transform.GetPositionAndRotation(out var currPos, out var currRot);

            // this is a position on an invisible circle around the user
            var newPosition = currPos + Quaternion.AngleAxis(angle + currRot.eulerAngles.y + CenteringRotation, Vector3.up) * Vector3.back * SnapshotDistance;

            var snapshot = CreateSnapshot(0, slicerPosition, slicerRotation);
            if (snapshot == null)
            {
                return;
            }
            snapshot.transform.position = newPosition;
        }
        
        public Snapshot? CreateSnapshot(ulong id, Vector3 slicerPosition, Quaternion slicerRotation)
        {
            var model = ModelManager.Instance.CurrentModel;

            var intersectionPoints = Slicer.GetIntersectionPointsFromWorld(model, slicerPosition, slicerRotation);
            if (intersectionPoints == null)
            {
                Debug.LogWarning("SlicePlane couldn't be created!");
                return null;
            }

            var dimensions = Slicer.GetTextureDimension(model, intersectionPoints);
            if (dimensions == null)
            {
                Debug.LogWarning("SliceCoords can't be calculated!");
                return null;
            }

            var texData = Slicer.CreateSliceTextureData(model, dimensions, intersectionPoints);
            var texture = Slicer.CreateSliceTexture(dimensions, texData);

            AudioManager.Instance.PlayCameraSound();

            var snapshot = Instantiate(snapshotPrefab, transform).GetComponent<Snapshot>();
            snapshot.ID = id;
            snapshot.SetIntersectionChild(texture, intersectionPoints.LowerLeft, model);

            var mainTransform = tabletOverlay.Main.transform;
            var originPlane = Instantiate(originPlanePrefab, mainTransform.position, mainTransform.rotation);
            originPlane.transform.SetParent(model.transform);
            originPlane.SetActive(false);

            snapshot.OriginPlane = originPlane;

            Snapshots.Add(snapshot);

            return snapshot;
        }

        public void DetachAllSnapshots()
        {
            foreach (var s in Snapshots)
            {
                if (s.IsAttached)
                {
                    s.Detach();
                }
            }
        }

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

            foreach (var s in Snapshots)
            {
                Destroy(s.gameObject);
            }

            Snapshots.Clear();

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
            Destroy(s.gameObject);
            SnapshotRemoved?.Invoke(s.ID);
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
    }
}