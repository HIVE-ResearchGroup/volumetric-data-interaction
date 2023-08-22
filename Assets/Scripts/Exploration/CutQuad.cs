using UnityEngine;

namespace Exploration
{
    public class CutQuad : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        public Mesh Mesh
        {
            set => _meshFilter.mesh = value;
        }

        public Material Material
        {
            set => _meshRenderer.material = value;
        }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}
