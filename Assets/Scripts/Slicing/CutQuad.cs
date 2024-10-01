#nullable enable

using UnityEngine;

namespace Slicing
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CutQuad : MonoBehaviour
    {
        private MeshFilter meshFilter = null!;
        private MeshRenderer meshRenderer = null!;

        public Mesh Mesh
        {
            set => meshFilter.mesh = value;
        }

        public Material Material
        {
            set => meshRenderer.material = value;
        }

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnDestroy()
        {
            Destroy(meshFilter.mesh);
            Destroy(meshRenderer.material);
        }
    }
}
