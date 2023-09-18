using Constants;
using Exploration;
using Helper;
using UnityEngine;

namespace Interaction
{
    public class Snapshot : MonoBehaviour
    {
        [SerializeField]
        private GameObject viewer;
        
        [SerializeField]
        private bool isLookingAt = true;
        
        [SerializeField]
        private GameObject originPlane;

        [SerializeField]
        private Material blackMaterial;
        
        [SerializeField]
        private Material mainUIMaterial;
        
        private GameObject _mainOverlay;
        private MeshRenderer _mainRenderer;
        private Texture _mainOverlayTexture;

        private Vector3 _misalignedPosition;
        private Vector3 _misalignedScale;

        private GameObject _tempNeighbourOverlay;

        public GameObject Viewer
        {
            set => viewer = value;
        }
        
        public bool IsLookingAt => isLookingAt;
        
        public GameObject OriginPlane
        {
            get => originPlane;
            set => originPlane = value;
        }
        
        public SlicePlaneCoordinates PlaneCoordinates { get; set; }
        
        public Texture SnapshotTexture { get; set; }

        public bool Selected
        {
            set
            {
                if (!originPlane)
                {
                    return;
                }
        
                originPlane.SetActive(value);
                SetOverlayTexture(value);
            }
        }

        private GameObject TextureQuad => transform.GetChild(0).gameObject;
        
        private void Awake()
        {
            _mainOverlay = GameObject.Find(StringConstants.Main);
            _mainRenderer = _mainOverlay.GetComponent<MeshRenderer>();
            _mainOverlayTexture = _mainRenderer.material.mainTexture;

            SnapshotTexture = TextureQuad.GetComponent<MeshRenderer>().material.mainTexture;
        }

        private void Update()
        {
            if (viewer && IsLookingAt)
            {
                var cachedTransform = transform;
                cachedTransform.LookAt(viewer.transform);
                cachedTransform.forward = -cachedTransform.forward; //need to adjust as quad is else not visible
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
            viewer = otherSnapshot.viewer;
            isLookingAt = false;
            _mainOverlay = otherSnapshot._mainOverlay;
            _mainOverlayTexture = otherSnapshot._mainOverlayTexture;
            SnapshotTexture = otherSnapshot.SnapshotTexture;
            _misalignedPosition = otherSnapshot._misalignedPosition;
            _misalignedScale = otherSnapshot._misalignedScale;
            originPlane = otherSnapshot.originPlane;
            originPlane.transform.position = originPlanePosition;
        }

        public void SetOverlayTexture(bool isSelected)
        {
            if (!_mainOverlay)
            {
                return;
            }

            if (isSelected)
            {
                _mainRenderer.material = blackMaterial;

                var overlay = _mainOverlay.transform;
                var snapshotQuad = Instantiate(TextureQuad);
                var cachedQuadTransform = snapshotQuad.transform;
                var cachedQuadScale = cachedQuadTransform.localScale;
                var scale = MaterialTools.GetAspectRatioSize(overlay.localScale, cachedQuadScale.y, cachedQuadScale.x); //new Vector3(1, 0.65f, 0.1f);
            
                cachedQuadTransform.SetParent(_mainOverlay.transform);
                cachedQuadTransform.localScale = scale;
                cachedQuadTransform.SetLocalPositionAndRotation(new Vector3(0, 0, -0.1f), new Quaternion());
                Destroy(_tempNeighbourOverlay);
                _tempNeighbourOverlay = snapshotQuad;
            }
            else
            {
                _mainRenderer.material = mainUIMaterial;
                Destroy(_tempNeighbourOverlay);
            }
        }

        public void SetAligned(Transform overlay)
        {
            var cachedTransform = transform;
            _misalignedScale = cachedTransform.localScale;
            _misalignedPosition = cachedTransform.localPosition;
            isLookingAt = false;
            transform.SetParent(overlay);
        }

        public void SetMisaligned()
        {
            var cachedTransform = transform;
            cachedTransform.SetParent(null);
            cachedTransform.localScale = _misalignedScale; 
            cachedTransform.position = _misalignedPosition;
            isLookingAt = true;
        }

        public void SetIntersectionChild(Texture2D texture, Vector3 startPoint, Model model)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(quad.GetComponent<MeshCollider>());

            var quadScale = MaterialTools.GetTextureAspectRatioSize(transform.localScale, texture);
            quad.transform.localScale = quadScale;

            quad.transform.SetParent(transform);
            quad.transform.localPosition = new Vector3(0, 0, 0.01f);

            var quadRenderer = quad.GetComponent<MeshRenderer>();
            quadRenderer.material.mainTexture = texture;
            quadRenderer.material = MaterialTools.GetMaterialOrientation(quadRenderer.material, model, startPoint);     
        }
    }
}