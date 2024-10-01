#nullable enable

using UnityEngine;

public static class MaterialTools
{
    // private const string ShaderOnePlane = "CrossSection/OnePlaneBSP";
    private const string ShaderStandard = "Standard";
    
    private static readonly Shader StandardShader = Shader.Find(ShaderStandard);
        
    public static Material GetMaterialOrientation(Material material, Model.Model modelScript, Vector3 startPoint)
    {
        if (modelScript.IsZEdgeVector(startPoint) && modelScript.IsYEdgeVector(startPoint)) // x-axis
        {
            material.SetTextureScale("_MainTex", new Vector2(-1f, -1f));
        }
        else if (modelScript.IsYEdgeVector(startPoint) && modelScript.IsXEdgeVector(startPoint)) // z-axis
        {
            material.SetTextureScale("_MainTex", new Vector2(1f, -1f));
        }
        else if (modelScript.IsZEdgeVector(startPoint) && modelScript.IsXEdgeVector(startPoint)) // y-axis
        {
            material.SetTextureScale("_MainTex", new Vector2(1f, -1f));
        }

        return material;
    }
    
    public static Vector3 GetTextureAspectRatioSize(Vector3 parentScale, Texture2D texture) => GetAspectRatioSize(parentScale, texture.height, texture.width);

    public static Vector3 GetAspectRatioSize(Vector3 parentScale, float height, float width)
    {
        var xScale = parentScale.x / width;

        var quadX = width * xScale;
        var quadY = height * xScale;
        var yScale = parentScale.y / quadY;

        if (yScale < 1)
        {
            quadX *= yScale;
            quadY *= yScale;
        }

        return new Vector3(quadX, quadY, 0.01f);
    }
        
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
