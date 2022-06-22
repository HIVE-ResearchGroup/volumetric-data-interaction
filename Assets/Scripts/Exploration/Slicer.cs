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
        public bool isTriggered; //using doubletap from extern
        private Material materialTemporarySlice;

        private bool isPermanentSlicing = true;

        private void Start()
        {
            materialTemporarySlice = Resources.Load(StringConstants.MaterialOnePlane, typeof(Material)) as Material;
        }

        private void Update()
        {
            if (isTouched || isTriggered)
            {
                isTriggered = false;
                if (isPermanentSlicing)
                {
                    SliceObject();
                    isPermanentSlicing = false;
                }
                else
                {
                    isTouched = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                isTriggered = true;
                isPermanentSlicing = true;
            }
        }

        private void SliceObject()
        {
            isTouched = false;

            Collider[] objectsToBeSliced = Physics.OverlapBox(transform.position, new Vector3(1, 0.1f, 0.1f), transform.rotation, sliceMask);

            foreach (Collider objectToBeSliced in objectsToBeSliced)
            {
                SlicedHull slicedObject = SliceObject(objectToBeSliced.gameObject, materialTemporarySlice);

                GameObject lowerHullGameobject = slicedObject.CreateUpperHull(objectToBeSliced.gameObject, materialTemporarySlice);
                lowerHullGameobject.transform.position = objectToBeSliced.transform.position;
                MakeItPhysical(lowerHullGameobject);

                Destroy(objectToBeSliced.gameObject);
                PrepareSliceModel(lowerHullGameobject);
            }
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