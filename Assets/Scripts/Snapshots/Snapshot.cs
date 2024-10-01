#nullable enable

using System;
using Selection;
using UnityEngine;

namespace Snapshots
{
    [RequireComponent(typeof(Selectable))]
    public class Snapshot : MonoBehaviour
    {
        private Vector3 detachedPosition;
        private Vector3 detachedScale;

        private GameObject? tempNeighbourOverlay;

        private GameObject textureQuad = null!;
        private MeshRenderer textureQuadRenderer = null!;

        private AttachmentPoint? attachmentPoint;

        public ulong ID { get; set; }

        public GameObject OriginPlane { get; set; } = null!;

        public Texture2D SnapshotTexture => textureQuadRenderer.material.mainTexture as Texture2D ?? throw new NullReferenceException("Snapshot texture was null!");

        public bool IsAttached => attachmentPoint != null;

        public Selectable Selectable { get; private set; } = null!;

        private void Awake()
        {
            tag = Tags.Snapshot;
            
            Selectable = GetComponent<Selectable>();
            textureQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(textureQuad.GetComponent<MeshCollider>());
            textureQuadRenderer = textureQuad.GetComponent<MeshRenderer>();
            textureQuad.transform.SetParent(transform);
            textureQuad.transform.localPosition = new Vector3(0, 0, 0.01f);
            textureQuad.SetActive(false);
        }

        private void OnEnable()
        {
            Selectable.SelectChanged += HandleSelection;
        }

        private void OnDisable()
        {
            Selectable.SelectChanged -= HandleSelection;
        }

        private void Update()
        {
            if (IsAttached)
            {
                return;
            }
            
            var cachedTransform = transform;
            cachedTransform.LookAt(ViewModeSetter.Instance.Camera.transform);
            cachedTransform.forward = -cachedTransform.forward; //need to adjust as quad is else not visible
        }

        private void OnDestroy()
        {
            Destroy(OriginPlane);
            if (tempNeighbourOverlay != null)
            {
                Destroy(tempNeighbourOverlay);
            }
        }

        public bool Attach()
        {
            if (attachmentPoint != null)
            {
                Debug.LogWarning("Snapshot already attached!");
                return true;
            }

            var ap = SnapshotManager.Instance.TabletOverlay.GetNextAttachmentPoint();
            if (ap == null)
            {
                Debug.LogError("No free attachment points!");
                return false;
            }

            attachmentPoint = ap;
            attachmentPoint.Attach(this);
            return true;
        }

        public void Detach()
        {
            if (attachmentPoint == null)
            {
                return;
            }
            
            attachmentPoint.Detach();
        }

        public void SetIntersectionChild(Texture2D texture, Vector3 startPoint, Model.Model model)
        {
            var quadScale = MaterialTools.GetTextureAspectRatioSize(transform.localScale, texture);
            textureQuad.transform.localScale = quadScale;

            var quadMaterial = textureQuadRenderer.material;
            quadMaterial.mainTexture = texture;
            textureQuadRenderer.material = MaterialTools.GetMaterialOrientation(quadMaterial, model, startPoint);
            
            textureQuad.SetActive(true);
        }

        //private void SetOverlayTexture(bool isSelected)
        //{
        //    if (isSelected)
        //    {
        //        var overlay = SnapshotManager.Instance.TabletOverlay.Main;
        //        var snapshotQuad = Instantiate(textureQuad);
        //        var cachedQuadTransform = snapshotQuad.transform;
        //        var cachedQuadScale = cachedQuadTransform.localScale;
        //        var scale = MaterialTools.GetAspectRatioSize(overlay.localScale, cachedQuadScale.y, cachedQuadScale.x); //new Vector3(1, 0.65f, 0.1f);
            
        //        cachedQuadTransform.SetParent(overlay);
        //        cachedQuadTransform.localScale = scale;
        //        cachedQuadTransform.SetLocalPositionAndRotation(new Vector3(0, 0, -0.1f), new Quaternion());
        //        Destroy(tempNeighbourOverlay);
        //        tempNeighbourOverlay = snapshotQuad;
        //    }
        //    else
        //    {
        //        Destroy(tempNeighbourOverlay);
        //    }
        //}

        private void HandleSelection(bool selected)
        {
            OriginPlane.SetActive(selected);
            //SetOverlayTexture(selected);
        }
    }
}