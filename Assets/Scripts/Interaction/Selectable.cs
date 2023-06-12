using Constants;
using Helper;
using Networking;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// Halo hack: https://answers.unity.com/questions/10534/changing-color-of-halo.html
    /// </summary>
    public class Selectable : MonoBehaviour
    {
        [SerializeField]
        private CollisionListener collisionListener;

        private Rigidbody _rigidbody;
        
        private Material greenMaterial;
        private Material highlightedMaterial;
        private Material defaultMaterial;

        private Host host;
        private bool isHighlighted = false;
        private MeshRenderer meshRenderer;

        private void OnEnable()
        {
            if (TryGetComponent(out Rigidbody rb))
            {
                _rigidbody = rb;
            }
        }

        private void Start()
        {
            host = FindObjectOfType<Host>();
            highlightedMaterial = Resources.Load<Material>(StringConstants.MaterialYellowHighlighted);
            greenMaterial = Resources.Load<Material>(StringConstants.MaterialGreen);

            if (TryGetComponent(out MeshRenderer ren))
            {
                meshRenderer = ren;
                defaultMaterial = ren.material;
            }
        }

        /// <summary>
        /// Selectables are only highlighted if there is not already a highlighted object marked as selected in host script.
        /// This should avoid selection overlap which could occur with overlapping objects.
        /// The first to be selected is the only to be selected.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (host.Highlighted != null || !other.name.Contains(StringConstants.Ray))
            {
                return;
            }

            isHighlighted = true;
            host.Highlighted = gameObject;
            SetMaterial(highlightedMaterial);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isHighlighted)
            {
                return;
            }

            isHighlighted = false;
            host.Highlighted = null;
            SetMaterial(defaultMaterial);
        }

        public void SetToDefault()
        {
            isHighlighted = false;
            SetMaterial(defaultMaterial);
            Freeze();
        }

        public void SetToSelected()
        {
            SetMaterial(greenMaterial);
        }

        public void Freeze() => _rigidbody.constraints = RigidbodyConstraints.FreezeAll;

        public void UnFreeze() => _rigidbody.constraints = RigidbodyConstraints.None;

        private void SetMaterial(Material newMaterial)
        {
            if (meshRenderer is null)
            {
                return;
            }

            meshRenderer.material = newMaterial;
            meshRenderer.material.mainTexture = defaultMaterial.mainTexture;
            meshRenderer.material.mainTextureScale = defaultMaterial.mainTextureScale;
        }
    }
}