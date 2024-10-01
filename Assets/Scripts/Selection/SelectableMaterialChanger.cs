#nullable enable

using UnityEngine;

namespace Selection
{
    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SelectableMaterialChanger : MonoBehaviour
    {
        [SerializeField]
        private Material selectedMaterial = null!;
        
        [SerializeField]
        private Material highlightedMaterial = null!;
        
        private Selectable selectable = null!;
        private MeshRenderer meshRenderer = null!;
        private Material defaultMaterial = null!;
        
        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            meshRenderer = GetComponent<MeshRenderer>();
            defaultMaterial = meshRenderer.material;
        }

        private void OnEnable()
        {
            selectable.HighlightChanged += OnHighlightChanged;
            selectable.SelectChanged += OnSelectChanged;
        }

        private void OnDisable()
        {
            selectable.HighlightChanged -= OnHighlightChanged;
            selectable.SelectChanged -= OnSelectChanged;
        }
        
        private void OnHighlightChanged(bool _) => UpdateTexture();

        private void OnSelectChanged(bool _) => UpdateTexture();

        private void UpdateTexture()
        {
            if (selectable.IsSelected)
            {
                SetMaterial(selectedMaterial);
            }
            else if (selectable.IsHighlighted)
            {
                SetMaterial(highlightedMaterial);
            }
            else
            {
                SetMaterial(defaultMaterial);
            }
        }

        private void SetMaterial(Material mat)
        {
            meshRenderer.material = mat;
            meshRenderer.material.mainTexture = defaultMaterial.mainTexture;
            meshRenderer.material.mainTextureScale = defaultMaterial.mainTextureScale;
        }
    }
}