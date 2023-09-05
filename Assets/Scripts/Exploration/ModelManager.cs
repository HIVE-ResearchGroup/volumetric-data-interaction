using Constants;
using Helper;
using Interaction;
using UnityEngine;

namespace Exploration
{
    public class ModelManager : MonoBehaviour
    {
        /*[SerializeField]
        private Model previousModel;*/

        [SerializeField]
        private Model model;
        
        private CollisionListener _listener;
        private BoxCollider _boxCollider;
        private OnePlaneCuttingController _cuttingController;
        private Renderer _modelRenderer;
        
        public Model CurrentModel { get; private set; }
        
        public static ModelManager Instance { get; private set; }

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
            _modelRenderer.material = material;
        }

        public void SetModelMaterial(Material material, Shader shader)
        {
            _modelRenderer.material = material;
            _modelRenderer.material.shader = shader;
        }

        public void ActivateCuttingPlane(GameObject plane)
        {
            _cuttingController.enabled = true;
            _cuttingController.plane = plane;
        }

        public void DeactivateCuttingPlane()
        {
            _cuttingController.enabled = false;
        }

        public void ResetModel()
        {
            // TODO
        }
    }
    
    /// <summary>
    /// Interactions class has to be attached to gameobject holding the tracked device
    /// </summary>
    public class Exploration : MonoBehaviour
    {
        public GameObject tracker;

        [SerializeField]
        private GameObject host;
        [SerializeField]
        private GameObject cuttingPlane;
        [SerializeField]
        private GameObject modelPrefab;

        /// <summary>
        /// e.g. 5 cm before HHD
        /// Do not allow multiple models!
        /// </summary>
        public GameObject CreateModel()
        {       
            var currTrackingPosition = tracker.transform.position;
            currTrackingPosition.z += 5;

            return CreateModel(currTrackingPosition, Quaternion.identity);
        }

        private GameObject CreateModel(Vector3 currPosition, Quaternion rotation)
        {
            var currModel = ModelManager.Instance.CurrentModel;
            if (currModel)
            {
                DeleteModel(currModel.gameObject);
            }

            Debug.Log($"** Create model with name {modelPrefab.name}.");
            var newModel = Instantiate(modelPrefab, currPosition, rotation);
            if (!cuttingPlane.TryGetComponent(out Slicer slicer))
            {
                slicer = cuttingPlane.AddComponent<Slicer>();
            }
            slicer.RegisterListener(newModel.GetComponent<CollisionListener>());
            newModel.name = StringConstants.ModelName;
            return newModel;
        }

        private static void DeleteModel(GameObject currModel)
        {
            if (!currModel)
            {
                return;
            }

            SnapshotInteraction.DeleteAllSnapshots();

            var modelName = currModel.name;
            Destroy(currModel);
            Debug.Log($"** Model with name {modelName} destroyed.");
        }

        public GameObject ResetModel()
        {
            var currModel = ModelManager.Instance.CurrentModel;
            if (!currModel)
            {
                Debug.Log("** There is no model to be reset!");
                return currModel.gameObject;
            }

            var position = currModel.transform.position;
            var rotation = currModel.transform.rotation;
            var currPosition = new Vector3(position.x, position.y, position.z);
            var currRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

            return CreateModel(currPosition, currRotation);
        }
    }
}
