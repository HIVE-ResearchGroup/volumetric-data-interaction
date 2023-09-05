using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Exploration;
using Extensions;
using Helper;
using Networking;
using UnityEngine;

namespace Interaction
{
    public class SnapshotInteraction : MonoBehaviour
    {
        private const float SnapshotThreshold = 3.0f;
        
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

        private float _snapshotTimer = 0.0f;

        private void Update()
        {
            if (_snapshotTimer <= SnapshotThreshold)
            {
                _snapshotTimer += Time.deltaTime;
            }
        }

        public void HandleSnapshotCreation(float angle)
        {
            if (SnapshotThreshold > _snapshotTimer) // means downward swipe - no placement
            {
                return;
            }

            _snapshotTimer = 0f;
            var currPos = tracker.transform.position;
            var currRot = tracker.transform.rotation;
            var centeringRotation = -90;

            var newPosition = currPos + Quaternion.AngleAxis(angle + currRot.eulerAngles.y + centeringRotation, Vector3.up) * Vector3.back * ConfigurationConstants.SNAPSHOT_DISTANCE;
            PlaceSnapshot(newPosition);
        }

        public bool HasSnapshots() => GetAllSnapshots().Any();

        public static bool IsSnapshot(GameObject selectedObject)
        {
            if (selectedObject == null)
            {
                return false;
            }
            return selectedObject.name.Contains(StringConstants.Snapshot) || IsSnapshotNeighbour(selectedObject);
        }

        private static bool IsSnapshotNeighbour(GameObject selectedObject) => selectedObject.name.Contains(StringConstants.Neighbour);

        // Get all snapshots without prefab
        private static IEnumerable<Snapshot> GetAllSnapshots() => Resources.FindObjectsOfTypeAll<Snapshot>()
            .Where(s => IsSnapshot(s.gameObject)
                        && s.gameObject.name.Contains(StringConstants.Clone));

        public static bool DeleteSnapshotsIfExist(Snapshot selectedObject, int shakeCounter)
        {
            if (selectedObject && IsSnapshot(selectedObject.gameObject)) {
                DeleteSnapshot(selectedObject);
                return true;
            }
            else if (shakeCounter > 1 && !selectedObject && GetAllSnapshots().Count() > 1)
            {
                DeleteAllSnapshots();
                return true;
            }
            return false;
        }

        public static void DeleteAllSnapshots() => GetAllSnapshots().ForEach(DeleteSnapshot);

        private static void DeleteSnapshot(Snapshot snapshot)
        {
            if (!snapshot.name.Contains(StringConstants.Clone))
            {
                return;
            }

            snapshot.SetSelected(false);
            Destroy(snapshot.OriginPlane);
            Destroy(snapshot.gameObject);
        }

        /// <summary>
        /// It could happen that nor all snapshots are aligned due to the size restriction
        /// </summary>
        private static bool AreSnapshotsAligned(IEnumerable<Snapshot> snapshots) => snapshots.Any(s => !s.IsLookingAt);

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

            //for (int index = 0; index < snapshots.Count() && index < 5; index++)
            foreach (var (shot, index) in snapshots.Take(5).Select((value, i) => (value, i)))
            {
                var child = tabletOverlay.GetChild(index + 1); // first child is main overlay
                shot.SetAligned(tabletOverlay);
                shot.transform.SetPositionAndRotation(child.position, new Quaternion());
                shot.transform.localScale = new Vector3(1, 0.65f, 0.1f);
            }
        }

        private static void MisalignSnapshots(IEnumerable<Snapshot> snapshots) => snapshots.ForEach(s => s.SetMisaligned());

        private void PlaceSnapshot(Vector3 newPosition)
        {
            var snapshot = Instantiate(snapshotPrefab).GetComponent<Snapshot>();
            snapshot.transform.position = newPosition;

            var model = ModelManager.Instance.CurrentModel;
            try
            {
                var slicePlane = model.GetIntersectionAndTexture();
                SetIntersectionChild(snapshot.gameObject, slicePlane.CalculateIntersectionPlane(), slicePlane.SlicePlaneCoordinates.StartPoint, model);
                snapshot.PlaneCoordinates = slicePlane.SlicePlaneCoordinates;
            }
            catch (Exception e)
            {
                Destroy(snapshot);
                Debug.LogError($"Error occured on snapshot creation: {e.Message}");
                return;
            }

            SetSnapshotScript(model, snapshot);
        }

        private void SetSnapshotScript(Model model, Snapshot snapshot)
        {
            var originPlane = Instantiate(originPlanePrefab, main.transform.position, main.transform.rotation);
            originPlane.transform.SetParent(model.transform);

            snapshot.Viewer = trackedCamera;
            snapshot.OriginPlane = originPlane;
            snapshot.SetSelected(false);
        }

        public void GetNeighbour(bool isLeft, GameObject selectedObject)
        {
            var overlay = tracker.transform.Find(StringConstants.OverlayScreen);
            if (!IsSnapshot(selectedObject) || overlay == null)
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
                var texture = intersectionPlane.HasValue ? intersectionPlane.Value.Texture : invalidTexture;
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

                SetIntersectionChild(neighbourGo, texture, startPoint, model);
                neighbourSnap.SetOverlayTexture(true);
                neighbourSnap.SetSelected(true);
                neighbourGo.SetActive(false);
            }
            catch (Exception)
            {
                Destroy(neighbourGo);
            }
        }

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

        private static void SetIntersectionChild(GameObject gameObject, Texture2D texture, Vector3 startPoint, Model model)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(quad.GetComponent<MeshCollider>());

            var quadScale = MaterialTools.GetTextureAspectRatioSize(gameObject.transform.localScale, texture);
            quad.transform.localScale = quadScale;

            quad.transform.SetParent(gameObject.transform);
            quad.transform.localPosition = new Vector3(0, 0, 0.01f);

            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.material.mainTexture = texture;
            renderer.material = MaterialTools.GetMaterialOrientation(renderer.material, model, startPoint);        
        }

        private GameObject CreateNeighbourGameObject()
        {
            var neighbourGo = Instantiate(snapshotPrefab);
            neighbourGo.name = StringConstants.Neighbour;
            Destroy(neighbourGo.GetComponent<Selectable>());
            return neighbourGo;
        }

        private static bool IsNeighbourStartPointDifferent(Vector3 originalStartpoint, Vector3 neighbourStartpoint) => originalStartpoint != neighbourStartpoint;

        public static void CleanUpNeighbours() => Resources.FindObjectsOfTypeAll<Snapshot>()
            .Where(s => IsSnapshotNeighbour(s.gameObject))
            .ForEach(s => Destroy(s.gameObject));

        public static void DeactivateAllSnapshots() => GetAllSnapshots().ForEach(s => s.SetSelected(false));
    }
}