using Constants;
using UnityEngine;

namespace Helper
{
    public static class MaterialCreator
    {
        private static readonly Shader StandardShader = Shader.Find(StringConstants.ShaderStandard);
        
        public static Material CreateTransparentMaterial()
        {
            var material = new Material(StandardShader);
            material.SetFloat("_Mode", 3);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = 3000;
            return material;
        }
    }
}