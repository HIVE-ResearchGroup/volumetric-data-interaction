using System;
using System.ComponentModel;
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
            return type switch
            {
                InterpolationType.Nearest => texture.GetPixel(x, y),
                InterpolationType.Bilinear => texture.GetPixelBilinear(texture.width / (float)x,
                    texture.height / (float)y),
                _ => throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(InterpolationType))
            };
        }
        
        private static (int x, int y) GetCoordinatePosition(int width, int height, int targetX, int targetY) => (Math.Clamp(targetX, 0, width - 1), Math.Clamp(targetY, 0, height - 1));
    }
}
