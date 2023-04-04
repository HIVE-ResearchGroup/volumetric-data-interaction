using System;
using UnityEngine;

public class CollisionListener : MonoBehaviour
{
    public event Action<Collider> OnCollisionEnter;
    public event Action<Collider> OnCollisionExit;

    private void OnTriggerEnter(Collider other)
    {
        OnCollisionEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        OnCollisionExit(other);
    }
}
