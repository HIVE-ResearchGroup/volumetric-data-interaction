using Assets.Scripts.Helper;
using EzySlice;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Exploration
{
    /// <summary>
    /// https://github.com/LandVr/SliceMeshes
    /// </summary>
    public class Slicer : MonoBehaviour
    {
        public bool isTouched;
        public bool isTriggered;
        public GameObject temporaryCuttingPlane;

        private GameObject model;
        private Material materialTemporarySlice;
        private Material materialWhite;
        private Material materialBlack;

        private void Start()
        {
            model = ModelFinder.FindModelGameObject();
            materialTemporarySlice = Resources.Load(StringConstants.MaterialOnePlane, typeof(Material)) as Material;
            materialWhite = Resources.Load(StringConstants.MaterialWhite, typeof(Material)) as Material;
            materialBlack = Resources.Load(StringConstants.MaterialBlack, typeof(Material)) as Material;
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
                model = ModelFinder.FindModelGameObject();
            }

            if (isActive)
            {
                OnePlaneCuttingController cuttingScript = model.AddComponent<OnePlaneCuttingController>();
                cuttingScript.plane = temporaryCuttingPlane;

                var modelRenderer = model.GetComponent<Renderer>();
                modelRenderer.material = materialTemporarySlice;
                modelRenderer.material.shader = Shader.Find(StringConstants.ShaderOnePlane);
            }
            else
            {
                Destroy(model.GetComponent<OnePlaneCuttingController>());
                model.GetComponent<Renderer>().material = materialWhite;
            }
        }

        private void SliceObject()
        {
            isTouched = false;
            isTriggered = false;

            Collider[] objectsToBeSliced = Physics.OverlapBox(transform.position, new Vector3(1, 0.1f, 0.1f), transform.rotation);
            
            if (!CalculateIntersectionImage(out Material sliceMaterial))
            {
                return;
            }

            foreach (Collider objectToBeSliced in objectsToBeSliced)
            {
                SlicedHull slicedObject = SliceObject(objectToBeSliced.gameObject);

                if (slicedObject == null) // e.g. collision with hand sphere
                {
                    continue;
                }

                GameObject lowerHullGameobject = slicedObject.CreateUpperHull(objectToBeSliced.gameObject, materialBlack);
                lowerHullGameobject.transform.position = objectToBeSliced.transform.position;
                MakeItPhysical(lowerHullGameobject);

                lowerHullGameobject = SetBoxCollider(lowerHullGameobject, objectToBeSliced);
                lowerHullGameobject = SwitchChildren(objectToBeSliced.gameObject, lowerHullGameobject);
                Destroy(objectToBeSliced.gameObject);
                PrepareSliceModel(lowerHullGameobject);
                SetIntersectionMesh(lowerHullGameobject, sliceMaterial);
            }
        }

        private GameObject SwitchChildren(GameObject oldObject, GameObject newObject)
        {
            var children = new List<Transform>();
            for (var i = 0; i < oldObject.transform.childCount; i++) {
                children.Add(oldObject.transform.GetChild(i));
            }

            children.ForEach(c => c.SetParent(newObject.transform));
            return newObject;
        }

        /// <summary>
        /// Original collider needs to be kept for the calculation of intersection points
        /// Remove mesh collider which is automatically set
        /// Only the original box collider is needed
        /// Otherwise the object will be duplicated!
        /// </summary>
        private GameObject SetBoxCollider(GameObject newObject, Collider oldObject)
        {
            var coll = newObject.AddComponent<BoxCollider>();
            var oldBoxCollider = oldObject as BoxCollider;
            coll.center = oldBoxCollider.center;
            coll.size = oldBoxCollider.size;

            Destroy(newObject.GetComponent<MeshCollider>());
            return newObject;
        }

        private bool CalculateIntersectionImage(out Material sliceMaterial)
        {
            var modelScript = model.GetComponent<Model>();
            Texture2D sliceTexture;
            SlicePlaneCoordinates intersection;
            try
            {
                (sliceTexture, intersection) = modelScript.GetIntersectionAndTexture();
            }
            catch
            {
                sliceMaterial = null;
                return false;
            }

            var transparentMaterial = CreateTransparentMaterial();
            transparentMaterial.name = "SliceMaterial";
            transparentMaterial.mainTexture = sliceTexture;

            sliceMaterial = MaterialAdjuster.GetMaterialOrientation(transparentMaterial, modelScript, intersection.StartPoint);
            return true;
        }

        private Material CreateTransparentMaterial()
        {
            var material = new Material(Shader.Find("Standard"));
            material.SetFloat("_Mode", 3);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = 3000;
            return material;
        }

        private void MakeItPhysical(GameObject obj)
        {
            obj.AddComponent<MeshCollider>().convex = true;
            var rigidbody = obj.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
        }

        private SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
        {
            return obj.Slice(transform.position, transform.forward, crossSectionMaterial);
        }

        private void PrepareSliceModel(GameObject model)
        {
            this.model = model;
            model.name = StringConstants.ModelName;
            model.AddComponent<Model>();
            var selectableScript = model.AddComponent<Selectable>();
            selectableScript.Freeze();

            // prepare for permanent slicing
            var listener = model.AddComponent<CollisionListener>();
            listener.AddEnterListener(_ => isTouched = true);
            listener.AddExitListener(_ => isTouched = false);

            // prepare for shader-temporary slicing
            OnePlaneCuttingController cuttingScript = model.AddComponent<OnePlaneCuttingController>();
            cuttingScript.plane = gameObject;
            ActivateTemporaryCuttingPlane(true);
        }

        private void SetIntersectionMesh(GameObject newModel, Material intersectionTexture)
        {
            var modelIntersection = new ModelIntersection(newModel, GameObject.Find(StringConstants.CuttingPlanePreQuad));
            var mesh = modelIntersection.CreateIntersectingMesh();
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "cut";
            Destroy(quad.GetComponent<MeshCollider>());
            quad.GetComponent<MeshFilter>().mesh = mesh;
            quad.transform.SetParent(newModel.transform);
            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.material = intersectionTexture;
        }
    }
}