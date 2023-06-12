using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helper
{
    public class CollisionListener : MonoBehaviour
    {
        private event Action<Collider> OnCollisionEnter;
        private event Action<Collider> OnCollisionExit;

        private readonly List<Action<Collider>> _enterListeners = new();
        private readonly List<Action<Collider>> _exitListeners = new();

        private void OnTriggerEnter(Collider other)
        {
            OnCollisionEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            OnCollisionExit?.Invoke(other);
        }

        public void AddEnterListener(Action<Collider> listener)
        {
            _enterListeners.Add(listener);
            OnCollisionEnter += listener;
        }

        public void AddExitListener(Action<Collider> listener)
        {
            _exitListeners.Add(listener);
            OnCollisionExit += listener;
        }

        public void RemoveEnterListener(Action<Collider> listener)
        {
            _enterListeners.Remove(listener);
            OnCollisionEnter -= listener;
        }

        public void RemoveExitListener(Action<Collider> listener)
        {
            _exitListeners.Remove(listener);
            OnCollisionExit -= listener;
        }

        private void OnDestroy()
        {
            foreach (var listener in _enterListeners)
            {
                OnCollisionEnter -= listener;
            }
            foreach (var listener in _exitListeners)
            {
                OnCollisionExit -= listener;
            }
        }
    }
}
