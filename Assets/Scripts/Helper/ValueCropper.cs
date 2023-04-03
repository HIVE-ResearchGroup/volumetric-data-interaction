using System;
using UnityEngine;

namespace Assets.Scripts.Helper
{
    public static class ValueCropper
    {
        public static Vector3 CropVector(Vector3 vector, Vector3 maxValueVector)
        {
            vector.x = Math.Clamp((int)Math.Round(vector.x, 0), 0, maxValueVector.x - 2);
            vector.y = Math.Clamp((int)Math.Round(vector.y, 0), 0, maxValueVector.y - 2);
            vector.z = Math.Clamp((int)Math.Round(vector.z, 0), 0, maxValueVector.z - 2);
            return vector;
        }

        public static (int x, int y, int z) CropIntVector(Vector3 vector, Vector3 maxValueVector)
        {
            vector = CropVector(vector, maxValueVector);
            return ((int)vector.x, (int)vector.y, (int)vector.z);
        }

        /// <summary>
        /// Need to apply threshold, as points are not always located at the very edge
        /// </summary>
        public static Vector3 ApplyThresholdCrop(Vector3 vector, Vector3 maxValueVector, float cropThreshold)
        {
            vector.x = CropWithThreshold(cropThreshold, vector.x, 0, maxValueVector.x);
            vector.y = CropWithThreshold(cropThreshold, vector.y, 0, maxValueVector.y);
            vector.z = CropWithThreshold(cropThreshold, vector.z, 0, maxValueVector.z);
            return vector;
        }

        public static float CropWithThreshold(float cropThreshold, float value, float min, float max)
        {
            if (value + max * cropThreshold >= max)
            {
                return max - 1;
            }
            else if (value - max * cropThreshold <= min)
            {
                return min;
            }
            return value;
        }
    }
}