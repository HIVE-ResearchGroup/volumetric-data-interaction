using EzySlice;
using System;
using System.Drawing.Imaging;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Exploration
{
    /// <summary>
    /// https://github.com/LandVr/SliceMeshes
    /// </summary>
    public class Slicer : MonoBehaviour
    {
        public LayerMask sliceMask;
        public bool isTouched;
        public bool isTriggered;

        private GameObject model;
        private Material materialTemporarySlice;

        private void Start()
        {
            materialTemporarySlice = Resources.Load(StringConstants.MaterialOnePlane, typeof(Material)) as Material;
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
                isTriggered = true;
            }
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);

            if (!model)
            {
                return;
            }

            if (isActive)
            {
                OnePlaneCuttingController cuttingScript = model.AddComponent<OnePlaneCuttingController>();
                cuttingScript.plane = gameObject;
            }
            else
            {
                Destroy(model.GetComponent<OnePlaneCuttingController>());
            }
        }

        private void SliceObject()
        {
            isTouched = false;
            isTriggered = false;

            Collider[] objectsToBeSliced = Physics.OverlapBox(transform.position, new Vector3(1, 0.1f, 0.1f), transform.rotation, sliceMask);
            var sliceTexture = CalculateIntersectionImage();

            //var intersectionMaterial = Resources.Load(StringConstants.MaterialWhite, typeof(Material)) as Material;
            var intersectionMaterial = materialTemporarySlice;
            intersectionMaterial.color = Color.white;
            intersectionMaterial.mainTexture = sliceTexture;

            foreach (Collider objectToBeSliced in objectsToBeSliced)
            {
                SlicedHull slicedObject = SliceObject(objectToBeSliced.gameObject, materialTemporarySlice);
                
                if (slicedObject == null) // e.g. collision with hand sphere
                {
                    continue;
                }

                GameObject lowerHullGameobject = slicedObject.CreateUpperHull(objectToBeSliced.gameObject, intersectionMaterial);
                lowerHullGameobject.transform.position = objectToBeSliced.transform.position;
                MakeItPhysical(lowerHullGameobject);

                // keep original collider size for intersection calculation
                var coll = lowerHullGameobject.AddComponent<BoxCollider>();
                var oldBoxCollider = objectToBeSliced as BoxCollider;
                coll.center = oldBoxCollider.center;
                coll.size = oldBoxCollider.size;

                Destroy(objectToBeSliced.gameObject);
                PrepareSliceModel(lowerHullGameobject);
            }
        }

        private Texture2D CalculateIntersectionImage()
        {
            var fullPlane = gameObject.transform.GetChild(0); // due to slicing the main plane might be incomplete, a full version is needed for intersection calculation
            var modelIntersection = new ModelIntersection(model, fullPlane.gameObject);
            var intersectionPoints = modelIntersection.GetNormalisedIntersectionPosition();
            var sliceCalculation = model.GetComponent<Model>().GetIntersectionPlane(intersectionPoints);

            var fileName = ("plane");
            //var fileName = DateTime.Now.ToString("yy-MM-dd-hh:mm:ss plane" );

            var fileLocation = Path.Combine(ConfigurationConstants.IMAGES_FOLDER_PATH, fileName);
            sliceCalculation.Save(fileLocation + ".bmp", ImageFormat.Bmp);

            Texture2D sliceTexture = Resources.Load(Path.Combine(StringConstants.Images, fileName)) as Texture2D;
            return sliceTexture;
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
            model.layer = LayerMask.NameToLayer(StringConstants.LayerSliceable);
            SliceListener sliceable = model.AddComponent<SliceListener>();
            sliceable.slicer = gameObject.GetComponent<Slicer>();

            // prepare for shader-temporary slicing
            OnePlaneCuttingController cuttingScript = model.AddComponent<OnePlaneCuttingController>();
            cuttingScript.plane = gameObject;
            var modelRenderer = model.GetComponent<Renderer>();
            modelRenderer.material = materialTemporarySlice;
            modelRenderer.material.shader = Shader.Find(StringConstants.ShaderOnePlane);
        }
    }
}