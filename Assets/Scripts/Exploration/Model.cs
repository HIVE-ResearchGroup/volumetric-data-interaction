using System;
using System.Collections.Generic;
using System.Drawing;
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
        private Bitmap[] originalBitmap;

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

        public Bitmap GetIntersectionPlane(List<Vector3> intersectionPoints, InterpolationType interpolation = InterpolationType.NearestNeighbour)
        {
            var points = new List<Vector3>();
            for (int i = 0; i < intersectionPoints.Count - 1; i++)
            {
                for (int j = i + 1; j < intersectionPoints.Count; j++)
                {
                    var v = Calculate2EdgeVectors(intersectionPoints[i], intersectionPoints[j]);
                    points.AddRange(v);
                }
            }

            var edgePoints = points.Distinct().Where(p => GetEdgePointCount(p) >= 2).ToList();
            if (edgePoints.Count < 3)
            {
                Debug.LogError("Cannot calculate a cutting plane with fewer than 3 coordinates");
            }

            var startPoint = edgePoints[0];
            var p1 = edgePoints[1];
            var p2 = edgePoints[edgePoints.Count == 8 ? 6 : 3];

            var diff1 = p1 - startPoint;
            var diff2 = p2 - startPoint;
            var (newWidth, newHeight) = GetDimensionsSyncDifferences(ref diff1, ref diff2);

            var xSteps = diff1 / newWidth;
            var ySteps = diff2 / newHeight;
            (xSteps, ySteps) = MinimiseSteps(xSteps, ySteps);

            var width = (int)Math.Round(Math.Abs(newWidth), 0);
            var height = (int)Math.Round(Math.Abs(newHeight), 0);

            var calculatedPlane =  CalculateIntersectionPlane(width, height, startPoint, xSteps, ySteps);
            return calculatedPlane;
        }

        public Bitmap CalculateIntersectionPlane(int width, int height, Vector3 startPoint, Vector3 xSteps, Vector3 ySteps)
        {
            var resultImage = new Bitmap(width, height);
            var currVector1 = startPoint;
            var currVector2 = startPoint;

            for (int w = 0; w < width; w++)
            {
                currVector1.x = (int)Math.Round(startPoint.x + w * xSteps.x, 0);
                currVector1.y = (int)Math.Round(startPoint.y + w * xSteps.y, 0);
                currVector1.z = (int)Math.Round(startPoint.z + w * xSteps.z, 0);

                for (int h = 0; h < height; h++)
                {
                    currVector2.x = (int)Math.Round(currVector1.x + h * ySteps.x, 0);
                    currVector2.y = (int)Math.Round(currVector1.y + h * ySteps.y, 0);
                    currVector2.z = (int)Math.Round(currVector1.z + h * ySteps.z, 0);

                    var croppedIndex = CropIntVector(currVector2);
                    var currBitmap = originalBitmap[croppedIndex.x];

                    System.Drawing.Color result;
                    // Use interpolation here (class & method already exist)
                    result = currBitmap.GetPixel((int)croppedIndex.z, (int)croppedIndex.y);

                    resultImage.SetPixel(w, h, result);
                }
            }

            return resultImage;
        }

        private (Vector3, Vector3) MinimiseSteps(Vector3 widthSteps, Vector3 heightSteps)
        {
            widthSteps.x = Math.Abs(widthSteps.x) < Math.Abs(heightSteps.x) ? 0 : widthSteps.x;
            heightSteps.x = Math.Abs(heightSteps.x) <= Math.Abs(widthSteps.x) ? 0 : heightSteps.x;

            widthSteps.y = Math.Abs(widthSteps.y) < Math.Abs(heightSteps.y) ? 0 : widthSteps.y;
            heightSteps.y = Math.Abs(heightSteps.y) <= Math.Abs(widthSteps.y) ? 0 : heightSteps.y;

            widthSteps.z = Math.Abs(widthSteps.z) < Math.Abs(heightSteps.z) ? 0 : widthSteps.z;
            heightSteps.z = Math.Abs(heightSteps.z) <= Math.Abs(widthSteps.z) ? 0 : heightSteps.z;

            return (widthSteps, heightSteps);
        }

        /// <summary>
        /// Method to get height and width dynamically
        /// Cannot use the biggest differences as these can be from the same coordinates
        /// Need to choose two coordinate axis
        /// Additional to the max difference, the additional width/height from possible angles must be calculated
        /// For this the third axis (which is not height or width) is used
        /// </summary>
        private (float max1, float max2) GetDimensionsSyncDifferences(ref Vector3 diffWidth, ref Vector3 diffHeight)
        {
            var listWidth = new List<float>() { diffWidth.x, diffWidth.y, diffWidth.z };
            var listHeight = new List<float>() { diffHeight.x, diffHeight.y, diffHeight.z };
            var indexSum = 3;

            var maxWidthIndex = GetIndexOfAbsHigherValue(listWidth);
            var maxHeightIndex = GetIndexOfAbsHigherValue(listHeight);

            var width = listWidth[maxWidthIndex];
            var height = listHeight[maxHeightIndex];

            var addIndex = (indexSum - maxWidthIndex - maxHeightIndex) % indexSum;
            var addWidth = listWidth[addIndex];
            var addHeight = listHeight[addIndex];

            var zeroVector = GetCustomZeroVector(maxWidthIndex);            
            if (maxWidthIndex == maxHeightIndex) // cannot use same coordinate for step calculation as a 2d image has 2 coordinates
            {
                listWidth.RemoveAt(maxWidthIndex);
                listHeight.RemoveAt(maxHeightIndex);
                indexSum = 1;

                maxWidthIndex = GetIndexOfAbsHigherValue(listWidth);
                maxHeightIndex = GetIndexOfAbsHigherValue(listHeight);
                var tempWidth = listWidth[maxWidthIndex];
                var tempHeight = listHeight[maxHeightIndex];

                if (Math.Abs(tempWidth) > Math.Abs(tempHeight)) {
                    width = tempWidth;
                    diffWidth.x *= zeroVector.x;
                    diffWidth.y *= zeroVector.y;
                    diffWidth.z *= zeroVector.z;
                    addIndex = indexSum - maxWidthIndex;
                }
                else
                {
                    height = tempHeight;
                    diffHeight.x *= zeroVector.x;
                    diffHeight.y *= zeroVector.y;
                    diffHeight.z *= zeroVector.z;
                    addIndex = indexSum - maxHeightIndex;
                }

                addHeight = listHeight[addIndex];
                addWidth = listWidth[addIndex];
            }

            return (Math.Abs(width) + Math.Abs(addWidth), Math.Abs(height) + Math.Abs(addHeight));
        }

        private Vector3 GetCustomZeroVector(int zeroOnIndex)
        {
            return new Vector3(zeroOnIndex == 0 ? 0 : 1, zeroOnIndex == 1 ? 0 : 1, zeroOnIndex == 2 ? 0 : 1);
        }

        private int GetIndexOfAbsHigherValue(List<float> values)
        {
            return values.IndexOf(GetAbsMaxValue(values));
        }

        private List<Vector3> Calculate2EdgeVectors(Vector3 p1, Vector3 p2)
        {
            var xDiff = p2.x - p1.x;
            var yDiff = p2.y - p1.y;
            var zDiff = p2.z - p1.z;

            var biggestDifference = Math.Abs(GetAbsMaxValue(xDiff, yDiff, zDiff));

            var xStep = xDiff / biggestDifference;
            var yStep = yDiff / biggestDifference;
            var zStep = zDiff / biggestDifference;

            var begin = IteratePoint(p1, xStep, yStep, zStep);
            var end = IteratePoint(p1, xStep * -1, yStep * -1, zStep * -1);

            return new List<Vector3>() { begin, end };
        }

        private float GetAbsMaxValue(float x, float y, float z)
        {
            return GetAbsMaxValue(new List<float>() { x, y, z });
        }

        private float GetAbsMaxValue(List<float> list)
        {
            var min = list.Min();
            var max = list.Max();
            return Mathf.Abs(min) > Mathf.Abs(max) ? min : max;
        }

        private Vector3 IteratePoint(Vector3 startPoint, float xStep, float yStep, float zStep)
        {
            startPoint = ApplyThresholdCrop(startPoint);
            var curr = startPoint;
            var prev = startPoint;
            var isValid = true;

            while (isValid)
            {
                curr.x += xStep;
                curr.y += yStep;
                curr.z += zStep;

                isValid = !IsVectorInvalid(curr);
                if (float.IsNaN(curr.x) || float.IsNaN(curr.y) || float.IsNaN(curr.z))
                {
                    curr = prev;
                    isValid = false;
                }
                else
                {
                    prev = curr;
                }
            }

            curr = CropVector(curr);
            return ApplyThresholdCrop(curr);
        }

        private bool IsVectorInvalid(Vector3 vector)
        {
            var isInvalid = vector.x < 0 || vector.x > xCount || float.IsNaN(vector.x);
            isInvalid |= vector.y < 0 || vector.y > yCount || float.IsNaN(vector.x);
            isInvalid |= vector.z < 0 || vector.z > zCount || float.IsNaN(vector.x);
            return isInvalid;
        }
        
        /// <summary>
        /// No need for x, as these are from the image slices
        /// Need to apply threshold, as points are not always located at the very edge
        /// </summary>
        private Vector3 ApplyThresholdCrop(Vector3 vector)
        {
            vector.x = CropWithThreshold(vector.x, 0, xCount);
            vector.y = CropWithThreshold(vector.y, 0, yCount);
            vector.z = CropWithThreshold(vector.z, 0, zCount);
            return vector;
        }

        private float CropWithThreshold(float value, float min, float max)
        {
            if (value + max*cropThreshold >= max)
            {
                return max - 1;
            }
            else if (value - max*cropThreshold <= min)
            {
                return min;
            }
            return value;
        }

        private Vector3 CropVector(Vector3 vector)
        {
            vector.x = CropValue((int) Math.Round(vector.x, 0), 0, xCount);
            vector.y = CropValue((int)Math.Round(vector.y, 0), 0, yCount);
            vector.z = CropValue((int)Math.Round(vector.z, 0), 0, zCount);
            return vector;
        }

        private (int x, int y, int z) CropIntVector(Vector3 vector)
        {
            vector = CropVector(vector);
            var x = (int)Math.Round(vector.x, 0);
            var y = (int)Math.Round(vector.y, 0);
            var z = (int)Math.Round(vector.z, 0);
            return (x, y, z);
            //return ((int)vector.x, (int)vector.y, (int)vector.z);
        }

        private float CropValue(float value, float min, float max)
        {
            return value < min ? min : value >= max ? max - 1 : value;
        }

        private int GetEdgePointCount(Vector3 point)
        {
            var count = 0;
            if (point.x <= 0 || (point.x + 1) >= xCount)
            {
                count++;
            }

            if (point.y <= 0 || (point.y + 1) >= yCount)
            {
                count++;
            }

            if (point.z <= 0 || (point.z + 1) >= zCount)
            {
                count++;
            }
            
            return count;
        }
    }
}