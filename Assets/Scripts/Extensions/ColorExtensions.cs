using UnityEngine;

namespace Extensions
{
    public static class ColorExtensions
    {
        private const float BlackTransparentThreshold = 0.12f; //30;
        
        public static Color MakeBlackTransparent(this Color color, float threshold = BlackTransparentThreshold)
        {
            if (color.r <= threshold
                && color.g <= threshold
                && color.b <= threshold)
            {
                return new Color(color.r, color.g, color.b, 0.0f);
            }

            return color;
        }
    }
}
