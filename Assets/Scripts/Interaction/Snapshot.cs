using Constants;
using Exploration;
using Helper;
using UnityEngine;

namespace Interaction
{
    public class Snapshot : MonoBehaviour
    {
        public GameObject Viewer;
        public bool IsLookingAt = true;
        public GameObject OriginPlane;

        [SerializeField]
        private Material blackMaterial;
        
        [SerializeField]
        private Material mainUIMaterial;
        
        private GameObject mainOverlay;
        private MeshRenderer mainRenderer;
        private Texture mainOverlayTexture;

        private Vector3 misalignedPosition;
        private Vector3 misalignedScale;

        private GameObject tempNeighbourOverlay;

        public SlicePlaneCoordinates PlaneCoordinates { get; set; }
        public Texture SnapshotTexture { get; set; }

        public bool Selected
        {
            set
            {
                if (!OriginPlane)
                {
                    return;
                }
        
                OriginPlane.SetActive(value);
                SetOverlayTexture(value);
            }
        }

        private GameObject TextureQuad => transform.GetChild(0).gameObject;
        
        private void Awake()
        {
            mainOverlay = GameObject.Find(StringConstants.Main);
            mainRenderer = mainOverlay.GetComponent<MeshRenderer>();
            mainOverlayTexture = mainRenderer.material.mainTexture;

            SnapshotTexture = TextureQuad.GetComponent<MeshRenderer>().material.mainTexture;
        }

        private void Update()
        {
            if (Viewer && IsLookingAt)
            {
                gameObject.transform.LookAt(Viewer.transform);
                transform.forward = -transform.forward; //need to adjust as quad is else not visible
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tags.Ray))
            {
                Selected = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(Tags.Ray))
            {
                Selected = false;
            }
        }

        public void InstantiateForGo(Snapshot otherSnapshot, Vector3 originPlanePosition)
        {
            Viewer = otherSnapshot.Viewer;
            IsLookingAt = false;
            mainOverlay = otherSnapshot.mainOverlay;
            mainOverlayTexture = otherSnapshot.mainOverlayTexture;
            SnapshotTexture = otherSnapshot.SnapshotTexture;
            misalignedPosition = otherSnapshot.misalignedPosition;
            misalignedScale = otherSnapshot.misalignedScale;
            OriginPlane = otherSnapshot.OriginPlane;
            OriginPlane.transform.position = originPlanePosition;
        }

        public void SetOverlayTexture(bool isSelected)
        {
            if (!mainOverlay)
            {
                return;
            }

            if (isSelected)
            {
                mainRenderer.material = blackMaterial;

                var overlay = mainOverlay.transform;
                var snapshotQuad = Instantiate(TextureQuad);
                var scale = MaterialTools.GetAspectRatioSize(overlay.localScale, snapshotQuad.transform.localScale.y, snapshotQuad.transform.localScale.x); //new Vector3(1, 0.65f, 0.1f);
            
                snapshotQuad.transform.SetParent(mainOverlay.transform);
                snapshotQuad.transform.localScale = scale;
                snapshotQuad.transform.SetLocalPositionAndRotation(new Vector3(0, 0, -0.1f), new Quaternion());
                Destroy(tempNeighbourOverlay);
                tempNeighbourOverlay = snapshotQuad;
            }
            else
            {
                mainRenderer.material = mainUIMaterial;
                Destroy(tempNeighbourOverlay);
            }
        }

        public void SetAligned(Transform overlay)
        {
            misalignedScale = transform.localScale;
            misalignedPosition = transform.localPosition;
            IsLookingAt = false;
            transform.SetParent(overlay);
        }

        public void SetMisaligned()
        {
            transform.SetParent(null);
            transform.localScale = misalignedScale; 
            transform.position = misalignedPosition;
            IsLookingAt = true;
        }
    }
}