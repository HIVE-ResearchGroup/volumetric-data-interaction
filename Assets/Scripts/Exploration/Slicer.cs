using EzySlice;
using System.Drawing.Imaging;
using System.IO;
using UnityEditor;
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

            var sliceMaterial = new Material(materialTemporarySlice);
            sliceMaterial.color = Color.white;
            sliceMaterial.mainTexture = CalculateIntersectionImage();

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
                Destroy(objectToBeSliced.gameObject);
                PrepareSliceModel(lowerHullGameobject);
            }
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

        private Texture2D CalculateIntersectionImage()
        {
            var fullPlane = gameObject.transform.GetChild(0); // due to slicing the main plane might be incomplete, a full version is needed for intersection calculation
            var modelIntersection = new ModelIntersection(model, fullPlane.gameObject);
            var intersectionPoints = modelIntersection.GetNormalisedIntersectionPosition();
            var sliceCalculation = model.GetComponent<Model>().GetIntersectionPlane(intersectionPoints);

            var extension = ".bmp";
            var fileName = ("currPlane");
            var fileLocation = Path.Combine(ConfigurationConstants.IMAGES_FOLDER_PATH, fileName + extension);

            // filename needs to exist to be loaded from resources!!
            // therefore only rename when next cutting plane is added
            var fileExists = File.Exists(fileLocation);
            if (fileExists)
            {
                var creation = File.GetCreationTime(fileLocation);
                var newName = Path.Combine(ConfigurationConstants.IMAGES_FOLDER_PATH, creation.ToString("yy-MM-dd hh.mm.ss plane") + extension);
                File.Move(fileLocation, newName);
            }

            sliceCalculation.Save(fileLocation, ImageFormat.Bmp);
            var resourceLocation = StringConstants.Images + "/" + fileName;
            
            Texture2D sliceTexture = Resources.Load(resourceLocation) as Texture2D;
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