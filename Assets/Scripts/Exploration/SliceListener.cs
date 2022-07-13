using UnityEngine;

namespace Assets.Scripts.Exploration
{
    /// <summary>
    /// https://github.com/LandVr/SliceMeshes
    /// </summary>
    public class SliceListener : MonoBehaviour
    {
        public Slicer slicer;

        private void OnTriggerEnter(Collider other)
        {
            slicer.isTouched = true;
        }

        private void OnCollisionEnter(Collision other)
        {
            Debug.Log("collision");
            slicer.isTouched = true;
        }
    }
}