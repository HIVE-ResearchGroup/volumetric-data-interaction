using System;
using System.IO;
using System.Linq;
using Constants;
using Helper;
using JetBrains.Annotations;
using Selection;
using Slicing;
using Snapshots;
using UnityEngine;

namespace Model
{
    /// <summary>
    /// Add scs.rsp to be able to use Bitmaps in Unity
    /// https://forum.unity.com/threads/using-bitmaps-in-unity.899168/
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class Model : MonoBehaviour
    {
        [SerializeField]
        private string stackPath = ConfigurationConstants.X_STACK_PATH_LOW_RES;

        [SerializeField]
        private GameObject sectionQuad;

        private MeshFilter _sectionQuadMeshFilter;
        private MeshFilter _meshFilter;
        private Renderer _renderer;
        private OnePlaneCuttingController _onePlaneCuttingController;

        private Mesh _originalMesh;
        
        private const float CropThreshold = 0.1f;
        
        public Selectable Selectable { get; private set; }
        
        public Collider Collider { get; private set; }
        
        public BoxCollider BoxCollider { get; private set; }

        public Texture2D[] OriginalBitmap { get; private set; }
        
        public int XCount { get; private set; }

        public int YCount { get; private set; }

        public int ZCount { get; private set; }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            Selectable = GetComponent<Selectable>();
            Collider = GetComponent<Collider>();
            BoxCollider = GetComponent<BoxCollider>();
            _renderer = GetComponent<Renderer>();
            _onePlaneCuttingController = GetComponent<OnePlaneCuttingController>();
            _sectionQuadMeshFilter = sectionQuad.GetComponent<MeshFilter>();
            
            OriginalBitmap = InitModel(stackPath);

            XCount = OriginalBitmap.Length;
            YCount = OriginalBitmap.Length > 0 ? OriginalBitmap[0].height : 0;
            ZCount = OriginalBitmap.Length > 0 ? OriginalBitmap[0].width : 0;

            _originalMesh = Instantiate(_meshFilter.sharedMesh);
        }

        private static Texture2D[] InitModel(string path)
        {
            if (!Directory.Exists(path))
            {
                return Array.Empty<Texture2D>();
            }
            var files = Directory.GetFiles(path);
            if (files.Length == 0)
            {
                Debug.LogWarning($"WARNING! No files loaded from: \"{path}\", check if the path exists");
                return Array.Empty<Texture2D>();
            }
            
            var model3D = new Texture2D[files.Length];

            for (var i = 0; i < files.Length; i++)
            {
                var imagePath = Path.Combine(path, files[i]);
                model3D[i] = FileTools.LoadImage(imagePath);
            }

            return model3D;
        }

        public Vector3 CountVector => new Vector3(XCount, YCount, ZCount);

        [CanBeNull]
        public SlicePlane GenerateSlicePlane()
        {
            var modelIntersection = new ModelIntersection(this, Collider, BoxCollider, sectionQuad, _sectionQuadMeshFilter);
            var intersectionPoints = modelIntersection.GetNormalisedIntersectionPosition();
            var validIntersectionPoints = intersectionPoints
                .Select(p => ValueCropper.ApplyThresholdCrop(p, CountVector, CropThreshold))
                .ToList();
            var slicePlane = SlicePlane.Create(this, validIntersectionPoints);
            if (slicePlane == null)
            {
                Debug.LogWarning("SlicePlane couldn't be created");
                return null;
            }
            
            AudioManager.Instance.PlayCameraSound();
            return slicePlane;
        }

        public bool IsXEdgeVector(Vector3 point) => point.x == 0 || (point.x + 1) >= XCount;

        public bool IsZEdgeVector(Vector3 point) => point.z == 0 || (point.z + 1) >= ZCount;

        public bool IsYEdgeVector(Vector3 point) => point.y == 0 || (point.y + 1) >= YCount;
        
        public void UpdateModel(Mesh newMesh, GameObject cuttingPlane)
        {
            Debug.Log("Replacing model");
            // TODO
            _onePlaneCuttingController.plane = cuttingPlane;
            _meshFilter.mesh = newMesh;
            Selectable.Freeze();
            //CurrentModel.OnePlaneCuttingController.plane = cuttingPlane;
            /*
            //objBase.AddComponent<MeshCollider>().convex = true;
            objBase.transform.position = previousModel.transform.position;
            objBase.name = StringConstants.ModelName;
            objBase.AddComponent<Rigidbody>().useGravity = false;

            /* Original collider needs to be kept for the calculation of intersection points
             * Remove mesh collider which is automatically set
             * Only the original box collider is needed
             * Otherwise the object will be duplicated!
             */
            /*
            _boxCollider = objBase.AddComponent<BoxCollider>();
            _boxCollider.center = previousModel.BoxCollider.center;
            _boxCollider.size = previousModel.BoxCollider.size;
            if (objBase.TryGetComponent(out MeshCollider meshCollider))
            {
                Destroy(meshCollider);
            }

            var oldTransform = previousModel.gameObject.transform;
            while (oldTransform.childCount > 0)
            {
                oldTransform.GetChild(oldTransform.childCount - 1).SetParent(objBase.transform);
            }

            Destroy(previousModel.gameObject);

            var model = objBase.AddComponent<Model>();
            var selectable = objBase.AddComponent<Selectable>();
            _listener = objBase.AddComponent<CollisionListener>();
            _cuttingController = objBase.AddComponent<OnePlaneCuttingController>();
            _modelRenderer = objBase.GetComponent<Renderer>();
            selectable.Freeze();
            _cuttingController.plane = cuttingPlane;

            previousModel = CurrentModel;
            CurrentModel = model;
            */
        }
        
        public void SetModelMaterial(Material material)
        {
            _renderer.material = material;
        }

        public void SetModelMaterial(Material material, Shader shader)
        {
            _renderer.material = material;
            _renderer.material.shader = shader;
        }
        
        public void ActivateCuttingPlane(GameObject plane)
        {
            _onePlaneCuttingController.enabled = true;
            _onePlaneCuttingController.plane = plane;
        }

        public void DeactivateCuttingPlane()
        {
            _onePlaneCuttingController.enabled = false;
        }
        
        public void ResetMesh()
        {
            _meshFilter.mesh = Instantiate(_originalMesh);
        }
    }
}