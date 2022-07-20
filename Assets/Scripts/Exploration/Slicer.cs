using EzySlice;
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
                // take over all textures?
            }
        }

        private void SliceObject()
        {
            isTouched = false;
            isTriggered = false;

            Collider[] objectsToBeSliced = Physics.OverlapBox(transform.position, new Vector3(1, 0.1f, 0.1f), transform.rotation, sliceMask);

            var modelRenderer = model.GetComponent<Renderer>();
            var sliceShader = Shader.Find(StringConstants.ShaderOnePlane);

            var sliceMaterial = new Material(Shader.Find("Standard")); //new Material(materialSlice);
            sliceMaterial.color = Color.white;
            sliceMaterial.name = "SliceMaterial";
            var sliceTexture = CalculateIntersectionImage();
            sliceMaterial.mainTexture = sliceTexture;
            sliceMaterial.SetTextureScale("_MainTex", new Vector2(-1, -1));
            //sliceMaterial.shader = sliceShader;

            //GameObject.Find("Main").GetComponent<Renderer>().material = sliceMaterial;

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

        /// <summary>
        /// Save to png and bitmap. Png as bitmap can not be loaded to a texture from unity
        /// Could be loaded using Resources.Load but with a big delay of multiple seconds
        /// No way to notive when Resources.Load is finsihed, only possible by overwriting existing images.
        /// </summary>
        /// <returns></returns>
        private Texture2D CalculateIntersectionImage()
        {

            var sliceTexture = model.GetComponent<Model>().GetIntersectionTexture();
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
            //var modelRenderer = model.GetComponent<Renderer>();
            //modelRenderer.material = materialTemporarySlice;
            //modelRenderer.material.shader = Shader.Find(StringConstants.ShaderOnePlane);
        }
    }
}