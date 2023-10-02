using Constants;
using EzySlice;
using Helper;
using Model;
using UnityEngine;

namespace Slicing
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

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tags.Model))
            {
                return;
            }

            _isTouched = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(Tags.Model))
            {
                return;
            }

            _isTouched = false;
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
            
            Debug.Log("Slicing");

            var cachedTransform = transform;
            var objectsToBeSliced = Physics.OverlapBox(cachedTransform.position, new Vector3(1, 0.1f, 0.1f), cachedTransform.rotation);
            
            if (!CalculateIntersectionImage(out var sliceMaterial))
            {
                Debug.LogWarning("Intersection image can't be calculated!");
                return;
            }

            foreach (var objectToBeSliced in objectsToBeSliced)
            {
                var slicedObject = objectToBeSliced.gameObject.Slice(cachedTransform.position, cachedTransform.forward);

                if (slicedObject == null) // e.g. collision with hand sphere
                {
                    Debug.Log("Nothing sliced");
                    continue;
                }

                Debug.Log($"Sliced gameobject \"{objectToBeSliced.name}\"");
                var lowerHull = slicedObject.CreateUpperHull(objectToBeSliced.gameObject, materialBlack);
                ModelManager.Instance.CurrentModel.UpdateModel(lowerHull.GetComponent<MeshFilter>().mesh, gameObject);
                Destroy(lowerHull);
                ActivateTemporaryCuttingPlane();
                SetIntersectionMesh(ModelManager.Instance.CurrentModel, sliceMaterial);
            }
        }
        
        public void ActivateTemporaryCuttingPlane()
        {
            temporaryCuttingPlane.SetActive(true);
            ModelManager.Instance.CurrentModel.ActivateCuttingPlane(temporaryCuttingPlane);
            ModelManager.Instance.CurrentModel.SetModelMaterial(materialTemporarySlice, materialShader);
        }

        public void DeactivateTemporaryCuttingPlane()
        {
            temporaryCuttingPlane.SetActive(false);
            ModelManager.Instance.CurrentModel.DeactivateCuttingPlane();
            ModelManager.Instance.CurrentModel.SetModelMaterial(materialWhite);
        }

        private static bool CalculateIntersectionImage(out Material sliceMaterial, InterpolationType interpolation = InterpolationType.Nearest)
        {
            var model = ModelManager.Instance.CurrentModel;
            var slicePlane = model.GenerateSlicePlane();
            if (slicePlane == null)
            {
                sliceMaterial = null;
                return false;
            }
            
            var transparentMaterial = MaterialTools.CreateTransparentMaterial();
            transparentMaterial.name = "SliceMaterial";
            transparentMaterial.mainTexture = slicePlane.CalculateIntersectionPlane(interpolationType: interpolation);
            sliceMaterial = MaterialTools.GetMaterialOrientation(transparentMaterial, model, slicePlane.SlicePlaneCoordinates.StartPoint);
            return true;
        }

        private void SetIntersectionMesh(Model.Model newModel, Material intersectionTexture)
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
    }
}