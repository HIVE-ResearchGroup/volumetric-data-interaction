using System;
using Constants;
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
        private Rigidbody _rigidbody;

        private bool _isHighlighted;
        private bool _isSelected;

        public event Action<bool> HighlightChanged;
        public event Action<bool> SelectChanged;

        private bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted == value)
                {
                    return;
                }
                
                _isHighlighted = value;
                HighlightChanged?.Invoke(_isHighlighted);
            }
        }
        private bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                SelectChanged?.Invoke(_isSelected);
            }
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
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

            Host.Instance.Highlighted = gameObject;
            IsHighlighted = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsHighlighted || !other.CompareTag(Tags.Ray))
            {
                return;
            }

            IsHighlighted = false;
            Host.Instance.Highlighted = null;
        }

        public void Select() => IsSelected = true;

        public void Unselect()
        {
            IsHighlighted = false;
            IsSelected = false;
            Freeze();
        }

        public void Freeze() => _rigidbody.constraints = RigidbodyConstraints.FreezeAll;

        public void UnFreeze() => _rigidbody.constraints = RigidbodyConstraints.None;
    }
}