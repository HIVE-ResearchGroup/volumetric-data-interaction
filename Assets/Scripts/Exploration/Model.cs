using Assets.Scripts.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly string stackPath = ConfigurationConstants.X_STACK_PATH_LOW_RES;

        public Texture2D[] originalBitmap;

        public int xCount; // number of images
        public int yCount; // img height
        public int zCount; // img width

        private float cropThreshold = 0.1f;

        private void Start()
        {
            LoadModel(stackPath);
        }

        public void LoadModel(string path)
        {
            originalBitmap = InitModel(path);

            xCount = originalBitmap.Length;
            yCount = originalBitmap.Length > 0 ? originalBitmap[0].height : 0;
            zCount = originalBitmap.Length > 0 ? originalBitmap[0].width : 0;
        }

        private Texture2D[] InitModel(string path)
        {
            if (!Directory.Exists(path))
            {
                return new Texture2D[0];
            }
            var files = Directory.GetFiles(path);
            Texture2D[] model3D = new Texture2D[files.Length];

            for (var i = 0; i < files.Length; i++)
            {
                var imagePath = Path.Combine(path, files[i]);
                model3D[i] = FileLoader.LoadImage(imagePath);
            }

            return model3D;
        }

        public Vector3 GetCountVector() => new Vector3(xCount, yCount, zCount);

        public List<Vector3> CalculateValidIntersectionPoints(List<Vector3> intersectionPoints)
        {
            var croppedIntersectionPoints = intersectionPoints.Select(p => ValueCropper.ApplyThresholdCrop(p, GetCountVector(), cropThreshold));
            
            if (croppedIntersectionPoints.Count() < 3)
            {
                throw new Exception("Cannot calculate a cutting plane with fewer than 3 coordinates");
            }

            return croppedIntersectionPoints.ToList();
        }

        private (Texture2D bitmap, SlicePlaneCoordinates plane) GetIntersectionPlane(List<Vector3> intersectionPoints, InterpolationType interpolation = InterpolationType.NearestNeighbour)
        {            
            var slicePlane = new SlicePlane(this, intersectionPoints);
            slicePlane.ActivateCalculationSound();

            var intersection = slicePlane.CalculateIntersectionPlane();
            return (intersection, slicePlane.SlicePlaneCoordinates);
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

        public static Texture2D LoadTexture(string fileLocation) => FileLoader.LoadImage($"{fileLocation}.png");
    
        public bool IsXEdgeVector(Vector3 point) => point.x == 0 || (point.x + 1) >= xCount;

        public bool IsZEdgeVector(Vector3 point) =>  point.z == 0 || (point.z + 1) >= zCount;

        public bool IsYEdgeVector(Vector3 point) => point.y == 0 || (point.y + 1) >= yCount;
    }
}