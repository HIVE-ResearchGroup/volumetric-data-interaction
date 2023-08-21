using Constants;
using UnityEngine;

namespace Extensions
{
    public static class ColorExtensions
    {
        public static Color MakeBlackTransparent(this Color color, float threshold = ConfigurationConstants.BLACK_TRANSPARENT_THRESHOLD)
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
