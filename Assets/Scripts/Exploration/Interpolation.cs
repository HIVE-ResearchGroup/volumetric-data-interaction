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
            switch (type)
            {
                case InterpolationType.NearestNeighbour:
                    return InterpolateNearestNeighbour(texture, targetX, targetY);
                case InterpolationType.Bilinear:
                    return InterpolateBilinear(texture, targetX, targetY);
                case InterpolationType.None:
                    return texture.GetPixel(targetX, targetY);
                default:
                    Debug.LogWarning($"Invalid Interpolation type: {type}");
                    return texture.GetPixel(targetX, targetY);
            }
        }

        private static Color InterpolateNearestNeighbour(Texture2D texture, int targetX, int targetY)
        {
            var (x, y) = GetCoordinatePosition(texture.width, texture.height, targetX, targetY);
            return texture.GetPixel(x, y);
        }
        
        private static Color InterpolateBilinear(Texture2D texture, int targetX, int targetY)
        {
            var (x, y) = GetCoordinatePosition(texture.width, texture.height, targetX, targetY);
            var u = texture.width / (float)x;
            var v = texture.height / (float)y;
            return texture.GetPixelBilinear(u, v);
        }
        
        private static (int x, int y) GetCoordinatePosition(int width, int height, int targetX, int targetY) => (Math.Clamp(targetX, 0, width - 1), Math.Clamp(targetY, 0, height - 1));
    }
}
