using System;
using Constants;
using Helper;
using Interaction;
using UnityEngine;

namespace Exploration
{
    public class ModelManager : MonoBehaviour
    {
        private static ModelManager _instance;

        private BoxCollider _boxCollider;
        private OnePlaneCuttingController _cuttingController;
        private Renderer _modelRenderer;
        
        public static ModelManager Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }
                var mmScript = new GameObject().AddComponent<ModelManager>();
                mmScript.name = nameof(ModelManager);
                DontDestroyOnLoad(mmScript.gameObject);
                _instance = mmScript;

                return _instance;
            }
        }
        
        public Model CurrentModel { get; private set; }

        private void Awake()
        {
            _instance = this;
        }

        public void ReplaceModel(GameObject objBase, BoxCollider oldCollider, Action<Collider> onEnter, Action<Collider> onExit, GameObject cuttingPlane)
        {
            //objBase.AddComponent<MeshCollider>().convex = true;
            objBase.transform.position = oldCollider.transform.position;
            objBase.name = StringConstants.ModelName;
            objBase.AddComponent<Rigidbody>().useGravity = false;

            /* Original collider needs to be kept for the calculation of intersection points
             * Remove mesh collider which is automatically set
             * Only the original box collider is needed
             * Otherwise the object will be duplicated!
             */
            _boxCollider = objBase.AddComponent<BoxCollider>();
            _boxCollider.center = oldCollider.center;
            _boxCollider.size = oldCollider.size;
            var meshCollider = objBase.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                Destroy(meshCollider);
            }

            var oldTransform = oldCollider.gameObject.transform;
            while (oldTransform.childCount > 0)
            {
                oldTransform.GetChild(oldTransform.childCount - 1).SetParent(objBase.transform);
            }
            
            Destroy(oldCollider.gameObject);
            
            var model = objBase.AddComponent<Model>();
            var selectable = objBase.AddComponent<Selectable>();
            var listener = objBase.AddComponent<CollisionListener>();
            _cuttingController = objBase.AddComponent<OnePlaneCuttingController>();
            _modelRenderer = objBase.GetComponent<Renderer>();
            selectable.Freeze();
            listener.AddEnterListener(onEnter);
            listener.AddExitListener(onExit);
            _cuttingController.plane = cuttingPlane;
            
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
