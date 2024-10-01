#nullable enable

using UnityEngine;

namespace Slicing
{
    public class IntersectionPoints
    {
        public Vector3 UpperLeft { get; init; }
        public Vector3 LowerLeft { get; init; }
        public Vector3 LowerRight { get; init; }
        public Vector3 UpperRight { get; init; }
    }
}