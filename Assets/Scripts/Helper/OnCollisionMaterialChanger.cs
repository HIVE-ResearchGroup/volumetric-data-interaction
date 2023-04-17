using UnityEngine;

namespace Assets.Scripts.Helper
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

        private void Start()
        {
            collisionListener.OnCollisionEnter += _ => meshRenderer.material = highlightedMaterial;
            collisionListener.OnCollisionExit += _ => meshRenderer.material = defaultMaterial;
        }
    }
}
