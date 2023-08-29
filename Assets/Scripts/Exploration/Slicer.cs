using EzySlice;
using Helper;
using UnityEngine;

namespace Exploration
{
    /// <summary>
    /// https://github.com/LandVr/SliceMeshes
    /// </summary>
    public class Slicer : MonoBehaviour
    {
        [SerializeField]
        private CutQuad cutQuadPrefab;
        
        [SerializeField]
        private GameObject temporaryCuttingPlane;

        [SerializeField]
        private GameObject cuttingPlane;
        
        [SerializeField]
        private Material materialTemporarySlice;
        
        [SerializeField]
        private Material materialWhite;
        
        [SerializeField]
        private Material materialBlack;
        
        [SerializeField]
        private Shader materialShader;
        
        private MeshFilter _cuttingPlaneMeshFilter;
        
        private bool _isTouched;

        private void Awake()
        {
            _cuttingPlaneMeshFilter = cuttingPlane.GetComponent<MeshFilter>();
        }

        public void RegisterListener(CollisionListener listener)
        {
            listener.AddEnterListener(OnListenerEnter);
            listener.AddExitListener(OnListenerExit);
        }

        public void UnregisterListener(CollisionListener listener)
        {
            listener.RemoveEnterListener(OnListenerEnter);
            listener.RemoveExitListener(OnListenerExit);
        }

        public void ActivateTemporaryCuttingPlane(bool isActive)
        {
            temporaryCuttingPlane.SetActive(isActive);

            if (isActive)
            {
                ModelManager.Instance.ActivateCuttingPlane(temporaryCuttingPlane);
                ModelManager.Instance.SetModelMaterial(materialTemporarySlice, materialShader);
            }
            else
            {
                ModelManager.Instance.DeactivateCuttingPlane();
                ModelManager.Instance.SetModelMaterial(materialWhite);
            }
        }

        public void Slice()
        {
            if (!_isTouched)
            {
                return;
            }
            // is this needed?
            // _isTouched should be automatically reset when sliced, because the collision listener is exiting
            //_isTouched = false;

            var cachedTransform = transform;
            var objectsToBeSliced = Physics.OverlapBox(cachedTransform.position, new Vector3(1, 0.1f, 0.1f), cachedTransform.rotation);
            
            if (!CalculateIntersectionImage(out var sliceMaterial))
            {
                return;
            }

            foreach (var objectToBeSliced in objectsToBeSliced)
            {
                var slicedObject = objectToBeSliced.gameObject.Slice(cachedTransform.position, cachedTransform.forward);

                if (slicedObject == null) // e.g. collision with hand sphere
                {
                    continue;
                }

                var lowerHull = slicedObject.CreateUpperHull(objectToBeSliced.gameObject, materialBlack);
                ModelManager.Instance.ReplaceModel(lowerHull, this, gameObject);
                Destroy(lowerHull);
                ActivateTemporaryCuttingPlane(true);
                SetIntersectionMesh(ModelManager.Instance.CurrentModel, sliceMaterial);
            }
        }

        private static bool CalculateIntersectionImage(out Material sliceMaterial, InterpolationType interpolation = InterpolationType.Nearest)
        {
            try
            {
                var model = ModelManager.Instance.CurrentModel;
                var slicePlane = model.GetIntersectionAndTexture();
                var transparentMaterial = MaterialTools.CreateTransparentMaterial();
                transparentMaterial.name = "SliceMaterial";
                transparentMaterial.mainTexture = slicePlane.CalculateIntersectionPlane(interpolationType: interpolation);
                sliceMaterial = MaterialTools.GetMaterialOrientation(transparentMaterial, model, slicePlane.SlicePlaneCoordinates.StartPoint);
                return true;
            }
            catch
            {
                sliceMaterial = null;
                return false;
            }
        }

        private void SetIntersectionMesh(Model newModel, Material intersectionTexture)
        {
            var modelIntersection = new ModelIntersection(newModel,
                newModel.Collider,
                newModel.BoxCollider,
                _cuttingPlaneMeshFilter.gameObject,
                _cuttingPlaneMeshFilter);
            var mesh = modelIntersection.CreateIntersectingMesh();

            var quad = Instantiate(cutQuadPrefab, newModel.transform);
            quad.name = "cut";
            quad.Mesh = mesh;
            quad.Material = intersectionTexture;
        }

        private void OnListenerEnter(Collider _) => _isTouched = true;

        private void OnListenerExit(Collider _) => _isTouched = false;
    }
}