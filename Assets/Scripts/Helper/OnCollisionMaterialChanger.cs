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
            collisionListener.AddEnterListener(_ => meshRenderer.material = highlightedMaterial);
            collisionListener.AddExitListener(_ => meshRenderer.material = defaultMaterial);
        }
    }
}
