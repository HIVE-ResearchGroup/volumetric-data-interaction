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

        private GameObject model;
        private Material materialTemporarySlice;
        private Material materialWhite;

        private void Start()
        {
            materialTemporarySlice = Resources.Load(StringConstants.MaterialOnePlane, typeof(Material)) as Material;            
            materialWhite = Resources.Load(StringConstants.MaterialWhite, typeof(Material)) as Material;            
            model = GameObject.Find(StringConstants.ModelName) ?? GameObject.Find($"{StringConstants.ModelName}{StringConstants.Clone}");
        }

        private void Update()
        {
            if (isTriggered && isTouched)
            {
                SliceObject();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                TriggerSlicing();
            }
        }

        public void TriggerSlicing()
        {
            Debug.Log("Slicing is triggered - touch: " + isTouched);
            isTriggered = true;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);

            if (!model)
            {
                model = GameObject.Find(StringConstants.ModelName) ?? GameObject.Find($"{StringConstants.ModelName}{StringConstants.Clone}");
            }

            if (isActive)
            {
                OnePlaneCuttingController cuttingScript = model.AddComponent<OnePlaneCuttingController>();
                cuttingScript.plane = gameObject;

                var modelRenderer = model.GetComponent<Renderer>();
                modelRenderer.material = materialTemporarySlice;
                modelRenderer.material.shader = Shader.Find(StringConstants.ShaderOnePlane);
            }
            else
            {
                Destroy(model.GetComponent<OnePlaneCuttingController>());
                var modelRenderer = model.GetComponent<Renderer>().material = materialWhite;
            }
        }

        private void SliceObject()
        {
            isTouched = false;
            isTriggered = false;

            Collider[] objectsToBeSliced = Physics.OverlapBox(transform.position, new Vector3(1, 0.1f, 0.1f), transform.rotation);
            var sliceMaterial = CalculateIntersectionImage();           

            foreach (Collider objectToBeSliced in objectsToBeSliced)
            {
                SlicedHull slicedObject = SliceObject(objectToBeSliced.gameObject);
                
                if (slicedObject == null) // e.g. collision with hand sphere
                {
                    continue;
                }

                GameObject lowerHullGameobject = slicedObject.CreateUpperHull(objectToBeSliced.gameObject, sliceMaterial);
                lowerHullGameobject.transform.position = objectToBeSliced.transform.position;
                MakeItPhysical(lowerHullGameobject);

                lowerHullGameobject = SetBoxCollider(lowerHullGameobject, objectToBeSliced);
                lowerHullGameobject = SwitchChildren(objectToBeSliced.gameObject, lowerHullGameobject);
                Destroy(objectToBeSliced.gameObject);
                PrepareSliceModel(lowerHullGameobject);
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

        private Material CalculateIntersectionImage()
        {
            var modelScript = model.GetComponent<Model>();
            var (sliceTexture, intersection) = modelScript.GetIntersectionAndTexture();

            var sliceMaterial = new Material(Shader.Find("Standard"));
            sliceMaterial.color = Color.white;
            sliceMaterial.name = "SliceMaterial";
            sliceMaterial.mainTexture = sliceTexture;

            if (intersection.StartPoint.z == 0 || (intersection.StartPoint.z + 1) >= modelScript.zCount)
            {
                sliceMaterial.SetTextureScale("_MainTex", new Vector2(-1.1f, 1.1f));
            }

            return sliceMaterial;
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
            SliceListener sliceable = model.AddComponent<SliceListener>();
            sliceable.slicer = gameObject.GetComponent<Slicer>();

            // prepare for shader-temporary slicing
            OnePlaneCuttingController cuttingScript = model.AddComponent<OnePlaneCuttingController>();
            cuttingScript.plane = gameObject;
        }
    }
}