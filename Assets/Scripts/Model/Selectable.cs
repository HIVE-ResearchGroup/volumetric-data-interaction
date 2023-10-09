using Constants;
using JetBrains.Annotations;
using Networking;
using UnityEngine;

namespace Model
{
    /// <summary>
    /// Halo hack: https://answers.unity.com/questions/10534/changing-color-of-halo.html
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Selectable : MonoBehaviour
    {
        [SerializeField]
        private Material greenMaterial;
        
        [SerializeField]
        private Material highlightedMaterial;
        
        private Rigidbody _rigidbody;
        [CanBeNull]
        private MeshRenderer _meshRenderer;
        [CanBeNull]
        private Material _defaultMaterial;

        private bool _isHighlighted;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (TryGetComponent(out MeshRenderer meshRenderer))
            {
                _meshRenderer = meshRenderer;
                _defaultMaterial = meshRenderer.material;
            }
            else
            {
                _meshRenderer = null;
                _defaultMaterial = null;
            }
        }

        /// <summary>
        /// Selectables are only highlighted if there is not already a highlighted object marked as selected in host script.
        /// This should avoid selection overlap which could occur with overlapping objects.
        /// The first to be selected is the only to be selected.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (Host.Instance.Highlighted != null || !other.CompareTag(Tags.Ray))
            {
                return;
            }

            _isHighlighted = true;
            Host.Instance.Highlighted = gameObject;
            SetMaterial(highlightedMaterial);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_isHighlighted || !other.CompareTag(Tags.Ray))
            {
                return;
            }

            _isHighlighted = false;
            Host.Instance.Highlighted = null;
            SetMaterial(_defaultMaterial);
        }

        public void Select()
        {
            SetMaterial(greenMaterial);
        }

        public void Unselect()
        {
            _isHighlighted = false;
            SetMaterial(_defaultMaterial);
            Freeze();
        }

        public void Freeze() => _rigidbody.constraints = RigidbodyConstraints.FreezeAll;

        public void UnFreeze() => _rigidbody.constraints = RigidbodyConstraints.None;

        private void SetMaterial(Material newMaterial)
        {
            if (_meshRenderer == null)
            {
                return;
            }
            
            _meshRenderer.material = newMaterial;
            _meshRenderer.material.mainTexture = _defaultMaterial.mainTexture;
            _meshRenderer.material.mainTextureScale = _defaultMaterial.mainTextureScale;
        }
    }
}