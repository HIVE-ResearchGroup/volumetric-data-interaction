using UnityEngine;

namespace Exploration
{

    public class SlicePlaneCoordinates
    {
        public int Width;
        public int Height;
        public Vector3 StartPoint;
        public Vector3 XSteps;
        public Vector3 YSteps;

        public SlicePlaneCoordinates(int width, int height, Vector3 startPoint, Vector3 xSteps, Vector3 ySteps)
        {
            Width = width;
            Height = height;
            StartPoint = startPoint;
            XSteps = xSteps;
            YSteps = ySteps;
        }

        public SlicePlaneCoordinates(SlicePlaneCoordinates plane, Vector3 startPoint) : this(plane.Width, plane.Height, startPoint, plane.XSteps, plane.YSteps) { }
    }
}