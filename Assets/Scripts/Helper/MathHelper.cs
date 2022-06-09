using System;

namespace Assets.Scripts.Helper
{
    public static class MathHelper
    {
        public static float CalculateRectangularTriangle(double angle, float distance)
        {
            return distance * (float)Math.Sin(angle);
        }

        public static float CalculatePytagoras(float distance, float y)
        {
            var distance_2 = Math.Pow(distance, 2);
            var y_2 = Math.Pow(y, 2);
            return (float) Math.Sqrt(distance_2 - y_2);
        }
    }
}
