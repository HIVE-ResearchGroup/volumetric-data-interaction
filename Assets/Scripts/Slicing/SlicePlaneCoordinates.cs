using UnityEngine;

namespace Slicing
{
    public class SlicePlaneCoordinates
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector3 StartPoint { get; set; }
        public Vector3 XSteps { get; set; }
        public Vector3 YSteps { get; set; }

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