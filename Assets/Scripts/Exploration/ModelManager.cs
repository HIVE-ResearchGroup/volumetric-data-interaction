using System;
using Constants;
using Helper;
using Interaction;
using UnityEngine;

namespace Exploration
{
    public class ModelManager : MonoBehaviour
    {
        [SerializeField]
        private Model previousModel;
        
        private CollisionListener _listener;
        private Action<Collider> _onEnterListener;
        private Action<Collider> _onExitListener;
        private BoxCollider _boxCollider;
        private OnePlaneCuttingController _cuttingController;
        private Renderer _modelRenderer;
        
        public static ModelManager Instance { get; private set; }
        
        public Model CurrentModel { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(Instance);
            }
            else
            {
                Destroy(this);
            }
        }

        public void ReplaceModel(GameObject objBase, Action<Collider> onEnter, Action<Collider> onExit, GameObject cuttingPlane)
        {
            //objBase.AddComponent<MeshCollider>().convex = true;
            objBase.transform.position = previousModel.transform.position;
            objBase.name = StringConstants.ModelName;
            objBase.AddComponent<Rigidbody>().useGravity = false;

            _listener.RemoveEnterListener(_onEnterListener);
            _listener.RemoveExitListener(_onExitListener);
            
            /* Original collider needs to be kept for the calculation of intersection points
             * Remove mesh collider which is automatically set
             * Only the original box collider is needed
             * Otherwise the object will be duplicated!
             */
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
            _onEnterListener = onEnter;
            _onExitListener = onExit;
            _listener.AddEnterListener(onEnter);
            _listener.AddExitListener(onExit);
            _cuttingController.plane = cuttingPlane;

            previousModel = CurrentModel;
            CurrentModel = model;
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
    }
}
