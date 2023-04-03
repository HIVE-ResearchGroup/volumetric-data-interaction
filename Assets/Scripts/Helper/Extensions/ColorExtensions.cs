using UnityEngine;

namespace Assets.Scripts.Helper.Extensions
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

        public static Color FromArgb(int value, Color color) => new Color(color.r, color.g, color.b, FromColorValue8Bit(value));

        public static int FromColorFloat(float value) => (int)(value * 255.0f);

        public static float FromColorValue8Bit(int value) => value / 255.0f;
    }
}
