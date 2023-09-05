using Constants;
using Networking;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// Halo hack: https://answers.unity.com/questions/10534/changing-color-of-halo.html
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Selectable : MonoBehaviour
    {
        [SerializeField]
        private Host host;
        
        [SerializeField]
        private Material greenMaterial;
        
        [SerializeField]
        private Material highlightedMaterial;
        
        private Rigidbody _rigidbody;
        private MeshRenderer _meshRenderer;
        private Material _defaultMaterial;

        private bool _isHighlighted;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _defaultMaterial = _meshRenderer.material;
        }

        /// <summary>
        /// Selectables are only highlighted if there is not already a highlighted object marked as selected in host script.
        /// This should avoid selection overlap which could occur with overlapping objects.
        /// The first to be selected is the only to be selected.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (host.Highlighted != null || !other.CompareTag(Tags.Ray))
            {
                return;
            }

            _isHighlighted = true;
            host.Highlighted = gameObject;
            SetMaterial(highlightedMaterial);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_isHighlighted)
            {
                return;
            }

            _isHighlighted = false;
            host.Highlighted = null;
            SetMaterial(_defaultMaterial);
        }

        public void SetToDefault()
        {
            _isHighlighted = false;
            SetMaterial(_defaultMaterial);
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
            _meshRenderer.material = newMaterial;
            _meshRenderer.material.mainTexture = _defaultMaterial.mainTexture;
            _meshRenderer.material.mainTextureScale = _defaultMaterial.mainTextureScale;
        }
    }
}