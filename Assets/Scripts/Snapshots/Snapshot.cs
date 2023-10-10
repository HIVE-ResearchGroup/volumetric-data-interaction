using Constants;
using Helper;
using Slicing;
using UnityEngine;

namespace Snapshots
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
        
        private Vector3 _misalignedPosition;
        private Vector3 _misalignedScale;

        private GameObject _tempNeighbourOverlay;
        
        private GameObject _textureQuad;
        private MeshRenderer _textureQuadRenderer;

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

        public Texture SnapshotTexture
        {
            get => _textureQuadRenderer.material.mainTexture;
            set => _textureQuadRenderer.material.mainTexture = value;
        }

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
        
        private void Awake()
        {
            _textureQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(_textureQuad.GetComponent<MeshCollider>());
            _textureQuadRenderer = _textureQuad.GetComponent<MeshRenderer>();
            _textureQuad.transform.SetParent(transform);
            _textureQuad.transform.localPosition = new Vector3(0, 0, 0.01f);
            _textureQuad.SetActive(false);
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
            SnapshotTexture = otherSnapshot.SnapshotTexture;
            _misalignedPosition = otherSnapshot._misalignedPosition;
            _misalignedScale = otherSnapshot._misalignedScale;
            originPlane = otherSnapshot.originPlane;
            originPlane.transform.position = originPlanePosition;
        }

        public void SetOverlayTexture(bool isSelected)
        {
            if (isSelected)
            {
                SnapshotManager.Instance.TabletOverlay.SetMaterial(blackMaterial);

                var overlay = SnapshotManager.Instance.TabletOverlay.Main;
                var snapshotQuad = Instantiate(_textureQuad);
                var cachedQuadTransform = snapshotQuad.transform;
                var cachedQuadScale = cachedQuadTransform.localScale;
                var scale = MaterialTools.GetAspectRatioSize(overlay.localScale, cachedQuadScale.y, cachedQuadScale.x); //new Vector3(1, 0.65f, 0.1f);
            
                cachedQuadTransform.SetParent(overlay);
                cachedQuadTransform.localScale = scale;
                cachedQuadTransform.SetLocalPositionAndRotation(new Vector3(0, 0, -0.1f), new Quaternion());
                Destroy(_tempNeighbourOverlay);
                _tempNeighbourOverlay = snapshotQuad;
            }
            else
            {
                SnapshotManager.Instance.TabletOverlay.SetMaterial(mainUIMaterial);
                Destroy(_tempNeighbourOverlay);
            }
        }

        public void SetAligned()
        {
            var cachedTransform = transform;
            _misalignedScale = cachedTransform.localScale;
            _misalignedPosition = cachedTransform.localPosition;
            isLookingAt = false;
            transform.SetParent(SnapshotManager.Instance.TabletOverlay.transform);
        }

        public void SetMisaligned()
        {
            var cachedTransform = transform;
            cachedTransform.SetParent(null);
            cachedTransform.localScale = _misalignedScale; 
            cachedTransform.position = _misalignedPosition;
            isLookingAt = true;
        }

        public void SetIntersectionChild(Texture2D texture, Vector3 startPoint, Model.Model model)
        {
            var quadScale = MaterialTools.GetTextureAspectRatioSize(transform.localScale, texture);
            _textureQuad.transform.localScale = quadScale;

            _textureQuadRenderer.material.mainTexture = texture;
            _textureQuadRenderer.material = MaterialTools.GetMaterialOrientation(_textureQuadRenderer.material, model, startPoint);
            
            _textureQuad.SetActive(true);
        }
    }
}