using Interaction;
using UnityEngine;

namespace Exploration
{
    public class ModelManager : MonoBehaviour
    {
        public static ModelManager Instance { get; private set; }

        [SerializeField]
        private Model model;
        
        public Model CurrentModel { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                CurrentModel = model;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        public void ReplaceModel(GameObject objBase, Slicer _, GameObject cuttingPlane)
        {
            // TODO
            CurrentModel.CuttingPlane = cuttingPlane;
            CurrentModel.Mesh = objBase.GetComponent<MeshFilter>().mesh;
            CurrentModel.Selectable.Freeze();
            //CurrentModel.OnePlaneCuttingController.plane = cuttingPlane;
            /*
            //objBase.AddComponent<MeshCollider>().convex = true;
            objBase.transform.position = previousModel.transform.position;
            objBase.name = StringConstants.ModelName;
            objBase.AddComponent<Rigidbody>().useGravity = false;

            slicer.UnregisterListener(_listener);

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
            slicer.RegisterListener(_listener);
            _cuttingController.plane = cuttingPlane;

            previousModel = CurrentModel;
            CurrentModel = model;
            */
        }

        public void SetModelMaterial(Material material)
        {
            CurrentModel.Material = material;
        }

        public void SetModelMaterial(Material material, Shader shader)
        {
            CurrentModel.Material = material;
            CurrentModel.Material.shader = shader;
        }

        public void ActivateCuttingPlane(GameObject plane)
        {
            CurrentModel.CuttingPlaneActive = true;
            CurrentModel.CuttingPlane = plane;
        }

        public void DeactivateCuttingPlane()
        {
            CurrentModel.CuttingPlaneActive = false;
        }

        public void ResetModel()
        {
            SnapshotManager.Instance.DeleteAllSnapshots();
            CurrentModel.ResetMesh();
        }
    }
}
