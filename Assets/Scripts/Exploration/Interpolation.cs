using System;
using UnityEngine;

namespace Exploration
{
    /// <summary>
    /// Takes care of calculation of interpolations.
    /// </summary>
    public static class Interpolation
    {
        public static Color Interpolate(InterpolationType type, Texture2D texture, int targetX, int targetY)
        {
            var (x, y) = GetCoordinatePosition(texture.width, texture.height, targetX, targetY);
            switch (type)
            {
                case InterpolationType.Nearest:
                    return texture.GetPixel(x, y);
                case InterpolationType.Bilinear:
                    return texture.GetPixelBilinear(texture.width / (float)x, texture.height / (float)y);
                default:
                    Debug.LogWarning($"Invalid Interpolation type: {type}");
                    return texture.GetPixel(x, y);
            }
        }
        
        private static (int x, int y) GetCoordinatePosition(int width, int height, int targetX, int targetY) => (Math.Clamp(targetX, 0, width - 1), Math.Clamp(targetY, 0, height - 1));
    }
}
