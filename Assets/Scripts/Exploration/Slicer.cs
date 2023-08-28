using Constants;
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

        private GameObject _cuttingPlane;
        private MeshFilter _cuttingPlaneMeshFilter;
        
        private Material materialTemporarySlice;
        private Material materialWhite;
        private Material materialBlack;
        private Shader materialShader;
        private Shader standardShader;
        
        private bool isTouched;

        private void Start()
        {
            _cuttingPlane = GameObject.Find(StringConstants.CuttingPlanePreQuad);
            _cuttingPlaneMeshFilter = _cuttingPlane.GetComponent<MeshFilter>();
            materialTemporarySlice = Resources.Load(StringConstants.MaterialOnePlane, typeof(Material)) as Material;
            materialWhite = Resources.Load(StringConstants.MaterialWhite, typeof(Material)) as Material;
            materialBlack = Resources.Load(StringConstants.MaterialBlack, typeof(Material)) as Material;
            materialShader = Shader.Find(StringConstants.ShaderOnePlane);
            standardShader = Shader.Find(StringConstants.ShaderStandard);
        }

        public void RegisterListener(CollisionListener listener)
        {
            listener.AddEnterListener(OnListenerEnter);
            listener.AddExitListener(OnListenerExit);
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
            if (!isTouched)
            {
                return;
            }
            isTouched = false;

            var objectsToBeSliced = Physics.OverlapBox(transform.position, new Vector3(1, 0.1f, 0.1f), transform.rotation);
            
            if (!CalculateIntersectionImage(out var sliceMaterial))
            {
                return;
            }

            foreach (var objectToBeSliced in objectsToBeSliced)
            {
                var slicedObject = objectToBeSliced.gameObject.Slice(transform.position, transform.forward);

                if (slicedObject == null) // e.g. collision with hand sphere
                {
                    continue;
                }

                var lowerHull = slicedObject.CreateUpperHull(objectToBeSliced.gameObject, materialBlack);
                ModelManager.Instance.ReplaceModel(lowerHull, OnListenerEnter, OnListenerExit, gameObject);
                ActivateTemporaryCuttingPlane(true);
                SetIntersectionMesh(ModelManager.Instance.CurrentModel, sliceMaterial);
            }
        }

        private bool CalculateIntersectionImage(out Material sliceMaterial, InterpolationType interpolation = InterpolationType.Nearest)
        {
            try
            {
                var model = ModelManager.Instance.CurrentModel;
                var slicePlane = model.GetIntersectionAndTexture();
                var transparentMaterial = MaterialCreator.CreateTransparentMaterial();
                transparentMaterial.name = "SliceMaterial";
                transparentMaterial.mainTexture = slicePlane.CalculateIntersectionPlane(interpolationType: interpolation);
                sliceMaterial = MaterialAdjuster.GetMaterialOrientation(transparentMaterial, model, slicePlane.SlicePlaneCoordinates.StartPoint);
                return true;
            }
            catch
            {
                sliceMaterial = null;
                return false;
            }
        }

        private Material CreateTransparentMaterial()
        {
            var material = new Material(standardShader);
            material.SetFloat("_Mode", 3);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = 3000;
            return material;
        }

        private void SetIntersectionMesh(Model newModel, Material intersectionTexture)
        {
            var modelIntersection = new ModelIntersection(newModel,
                newModel.Collider,
                newModel.BoxCollider,
                _cuttingPlane,
                _cuttingPlaneMeshFilter);
            var mesh = modelIntersection.CreateIntersectingMesh();

            var quad = Instantiate(cutQuadPrefab, newModel.transform);
            quad.name = "cut";
            quad.Mesh = mesh;
            quad.Material = intersectionTexture;
        }

        private void OnListenerEnter(Collider _) => isTouched = true;

        private void OnListenerExit(Collider _) => isTouched = false;
    }
}