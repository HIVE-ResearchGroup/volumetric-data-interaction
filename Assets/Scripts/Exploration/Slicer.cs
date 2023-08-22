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
        
        public bool isTouched;
        public bool isTriggered;
        public GameObject temporaryCuttingPlane;

        private GameObject _cuttingPlane;
        private MeshFilter _cuttingPlaneMeshFilter;
        
        private GameObject model;
        private Material materialTemporarySlice;
        private Material materialWhite;
        private Material materialBlack;
        private Shader materialShader;
        private Shader standardShader;

        private void Start()
        {
            //model = ModelFinder.FindModelGameObject();
            _cuttingPlane = GameObject.Find(StringConstants.CuttingPlanePreQuad);
            _cuttingPlaneMeshFilter = _cuttingPlane.GetComponent<MeshFilter>();
            model = ModelManager.Instance.CurrentModel.gameObject;
            materialTemporarySlice = Resources.Load(StringConstants.MaterialOnePlane, typeof(Material)) as Material;
            materialWhite = Resources.Load(StringConstants.MaterialWhite, typeof(Material)) as Material;
            materialBlack = Resources.Load(StringConstants.MaterialBlack, typeof(Material)) as Material;
            materialShader = Shader.Find(StringConstants.ShaderOnePlane);
            standardShader = Shader.Find("Standard");
        }

        private void Update()
        {
            if (isTriggered && isTouched)
            {
                SliceObject();
            }
        }

        public void TriggerSlicing()
        {
            isTriggered = true;           
        }         

        public void ActivateTemporaryCuttingPlane(bool isActive)
        {
            temporaryCuttingPlane.SetActive(isActive);

            if (!model)
            {
                //model = ModelFinder.FindModelGameObject();
                model = ModelManager.Instance.CurrentModel.gameObject;
            }

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

        private void SliceObject()
        {
            isTouched = false;
            isTriggered = false;

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
                ModelManager.Instance.ReplaceModel(lowerHull, objectToBeSliced as BoxCollider, OnListenerEnter, OnListenerExit, gameObject);
                ActivateTemporaryCuttingPlane(true);
                SetIntersectionMesh(ModelManager.Instance.CurrentModel, sliceMaterial);
            }
        }

        private bool CalculateIntersectionImage(out Material sliceMaterial, InterpolationType interpolation = InterpolationType.Nearest)
        {
            var modelScript = model.GetComponent<Model>();
            try
            {
                var slicePlane = modelScript.GetIntersectionAndTexture();
                var transparentMaterial = CreateTransparentMaterial();
                transparentMaterial.name = "SliceMaterial";
                transparentMaterial.mainTexture = slicePlane.CalculateIntersectionPlane(interpolationType: interpolation);
                sliceMaterial = MaterialAdjuster.GetMaterialOrientation(transparentMaterial, modelScript, slicePlane.SlicePlaneCoordinates.StartPoint);
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