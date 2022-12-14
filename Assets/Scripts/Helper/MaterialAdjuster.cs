using Assets.Scripts.Exploration;
using UnityEngine;

namespace Assets.Scripts.Helper
{
    public static class MaterialAdjuster
    {
        public static Material GetMaterialOrientation(Material material, Model modelScript, Vector3 startPoint)
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
    }
}
