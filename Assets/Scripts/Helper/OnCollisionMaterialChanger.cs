using UnityEngine;

namespace Helper
{
    public class OnCollisionMaterialChanger : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer meshRenderer;
        [SerializeField]
        private CollisionListener collisionListener;

        [SerializeField]
        private Material defaultMaterial;
        [SerializeField]
        private Material highlightedMaterial;

        private void OnEnable()
        {
            collisionListener.AddEnterListener(Highlight);
            collisionListener.AddExitListener(Unhighlight);
        }

        private void OnDisable()
        {
            collisionListener.RemoveEnterListener(Highlight);
            collisionListener.RemoveExitListener(Unhighlight);
        }

        private void Highlight(Collider _)
        {
            meshRenderer.material = highlightedMaterial;
        }

        private void Unhighlight(Collider _)
        {
            meshRenderer.material = defaultMaterial;
        }
    }
}
