using Constants;
using UnityEngine;

namespace Extensions
{
    public static class ColorExtensions
    {
        public static int ToArgb(this Color color) => (FromColorFloat(color.a) << 24)
                                                    + (FromColorFloat(color.r) << 16)
                                                    + (FromColorFloat(color.g) << 8)
                                                    + FromColorFloat(color.b);

        public static Color FromArgb(int value) => new Color(FromColorValue8Bit(value >> 16 & 0xFF),
                                                             FromColorValue8Bit(value >> 8 & 0xFF),
                                                             FromColorValue8Bit(value & 0xFF),
                                                             FromColorValue8Bit(value >> 24 & 0xFF));

        public static Color MakeBlackTransparent(this Color color, float threshold = ConfigurationConstants.BLACK_TRANSPARENT_THRESHOLD)
        {
            if (color.r <= threshold
                && color.g <= threshold
                && color.b <= threshold)
            {
                return FromArgb(0.0f, color);
            }

            return color;
        }
        
        private static Color FromArgb(float value, Color color) => new Color(color.r, color.g, color.b, value);

        private static int FromColorFloat(float value) => (int)(value * 255.0f);

        private static float FromColorValue8Bit(int value) => value / 255.0f;
    }
}
