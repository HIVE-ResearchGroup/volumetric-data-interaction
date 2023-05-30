using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helper
{
    public class CollisionListener : MonoBehaviour
    {
        private event Action<Collider> OnCollisionEnter;
        private event Action<Collider> OnCollisionExit;

        private List<Action<Collider>> enterListeners = new List<Action<Collider>>();
        private List<Action<Collider>> exitListeners = new List<Action<Collider>>();

        private void OnTriggerEnter(Collider other)
        {
            OnCollisionEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            OnCollisionExit(other);
        }

        public void AddEnterListener(Action<Collider> listener)
        {
            enterListeners.Add(listener);
            OnCollisionEnter += listener;
        }

        public void AddExitListener(Action<Collider> listener)
        {
            exitListeners.Add(listener);
            OnCollisionExit += listener;
        }

        private void OnDestroy()
        {
            foreach (var listener in enterListeners)
            {
                OnCollisionEnter -= listener;
            }
            foreach (var listener in exitListeners)
            {
                OnCollisionExit -= listener;
            }
        }
    }
}
