using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Extensions
{
    public static class TransformExtensions
    {
        public static Vector3 left(this Transform t) => t.right * -1;

        public static Vector3 down(this Transform t) => t.up * -1;

        public static Vector3 back(this Transform t) => t.forward * -1;
    }
}