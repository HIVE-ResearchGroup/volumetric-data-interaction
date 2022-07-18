using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Exploration
{
    /// <summary>
    /// Add scs.rsp to be able to use Bitmaps in Unity
    /// https://forum.unity.com/threads/using-bitmaps-in-unity.899168/
    /// </summary>
    public class Model : MonoBehaviour
    {
        [SerializeField]
        public Bitmap[] originalBitmap;

        public int xCount; // number of images
        public int yCount; // img height
        public int zCount; // img width

        private float cropThreshold = 0.1f;

        public Model()
        {
            originalBitmap = InitModel(ConfigurationConstants.X_STACK_PATH_LOW_RES);           
        }

        private Bitmap[] InitModel(string path)
        {
            var files = Directory.GetFiles(path);
            Bitmap[] model3D = new Bitmap[files.Length];

            for (var i = 0; i < files.Length; i++)
            {
                var imagePath = Path.Combine(path, files[i]);
                Console.WriteLine(imagePath);
                model3D[i] = new Bitmap(imagePath);
            }

            
            xCount = files.Length;
            yCount = model3D[0].Height;
            zCount = model3D[0].Width;

            return model3D;
        }

        public Vector3 GetCountVector() => new Vector3(xCount, yCount, zCount);

        public Bitmap GetIntersectionPlane(List<Vector3> intersectionPoints, InterpolationType interpolation = InterpolationType.NearestNeighbour)
        {
            intersectionPoints.ForEach(p => ValueCropper.ApplyThresholdCrop(p, GetCountVector(), cropThreshold));

            var slicePlane = new SlicePlane(this, intersectionPoints);
            var intersection = slicePlane.CalculateIntersectionPlane();
            return intersection;
        }
    }
}