using Assets.Scripts.Helper;
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
                Debug.Log(imagePath);
                model3D[i] = new Bitmap(imagePath);
            }


            xCount = files.Length;
            yCount = model3D[0].Height;
            zCount = model3D[0].Width;

            return model3D;
        }

        public Vector3 GetCountVector() => new Vector3(xCount, yCount, zCount);

        public List<Vector3> CalculateValidIntersectionPoints(List<Vector3> intersectionPoints)
        {
            List<Vector3> croppedIntersectionPoints = new List<Vector3>();
            intersectionPoints.ForEach(p => croppedIntersectionPoints.Add(ValueCropper.ApplyThresholdCrop(p, GetCountVector(), cropThreshold)));
            
            if (croppedIntersectionPoints.Count < 3)
            {
                throw new Exception("Cannot calculate a cutting plane with fewer than 3 coordinates");
            }

            return croppedIntersectionPoints;
        }

        private (Bitmap bitmap, SlicePlaneCoordinates plane) GetIntersectionPlane(List<Vector3> intersectionPoints, InterpolationType interpolation = InterpolationType.NearestNeighbour)
        {            
            var slicePlane = new SlicePlane(this, intersectionPoints);
            slicePlane.ActivateCalculationSound();

            var intersection = slicePlane.CalculateIntersectionPlane();
            return (intersection, slicePlane.GetSlicePlaneCoordinates());
        }

        public (Texture2D texture, SlicePlaneCoordinates plane) GetIntersectionAndTexture(InterpolationType interpolation = InterpolationType.NearestNeighbour)
        {
            var sectionQuadFull = GameObject.Find(StringConstants.SectionQuad).transform.GetChild(0); // due to slicing the main plane might be incomplete, a full version is needed for intersection calculation
            var modelIntersection = new ModelIntersection(gameObject, sectionQuadFull.gameObject);
            var intersectionPoints = modelIntersection.GetNormalisedIntersectionPosition();

            var validIntersectionPoints = CalculateValidIntersectionPoints(intersectionPoints);
            var (sliceCalculation, plane) = GetIntersectionPlane(validIntersectionPoints, interpolation);

            var fileLocation = FileSetter.SaveBitmapPng(sliceCalculation);
            var sliceTexture = LoadTexture(fileLocation);
            return (sliceTexture, plane);
        }

        public static Texture2D LoadTexture(string fileLocation)
        {
            var bytes = File.ReadAllBytes(fileLocation + ".png");
            var sliceTexture = new Texture2D(1, 1);
            sliceTexture.LoadImage(bytes);
            return sliceTexture;
        }
    
        public bool IsXEdgeVector(Vector3 point)
        {
            return point.x == 0 || (point.x + 1) >= xCount;
        }

        public bool IsZEdgeVector(Vector3 point)
        {
            return point.z == 0 || (point.z + 1) >= zCount;
        }

        public bool IsYEdgeVector(Vector3 point)
        {
            return point.y == 0 || (point.y + 1) >= yCount;
        }

        public static GameObject GetModelGameObject()
        {
            return GameObject.Find(StringConstants.ModelName) ?? GameObject.Find($"{StringConstants.ModelName}{StringConstants.Clone}");
        }

    }
}