using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Extensions;
using Helper;
using UnityEngine;

namespace Exploration
{
    public class SlicePlane
    {
        private readonly AudioSource _audioSource;
        private readonly AudioClip _cameraSound;
        private readonly Texture2D _invalidTexture;

        private Model _model;
        
        public SlicePlane(Model model, SlicePlaneCoordinates plane) : this(model)
        {
            SlicePlaneCoordinates = plane;
        }

        public SlicePlane(Model model, List<Vector3> intersectionPoints) : this(model)
        {
            SlicePlaneCoordinates = GetSliceCoordinates(intersectionPoints);
        }

        private SlicePlane(Model model)
        {
            _model = model;
            _audioSource = GameObject.Find(StringConstants.AudioSource).GetComponent<AudioSource>();
            _cameraSound = Resources.Load<AudioClip>(StringConstants.SoundCamera);
            _invalidTexture = Resources.Load<Texture2D>(StringConstants.ImageInvalid);
            HandleEmptyModelBitmap();
        }

        public SlicePlaneCoordinates SlicePlaneCoordinates { get; }

        /// <summary>
        /// It could happen that the originalbitmap get emptied in the process
        /// It therefore needs to be refilled
        /// </summary>
        private void HandleEmptyModelBitmap()
        {
            if (_model.originalBitmap.Length == 0)
            {
                var go = _model.gameObject;
                if (go.TryGetComponent(out Model oldModel))
                {
                    GameObject.Destroy(oldModel);
                }
                _model = go.AddComponent<Model>();
            }
        }

        private SlicePlaneCoordinates GetSliceCoordinates(List<Vector3> intersectionPoints)
        {
            var planeFormula = new PlaneFormula(intersectionPoints);

            var edgePoints = CalculateEdgePoints(planeFormula).ToList();

            if (edgePoints.Count < 3)
            {
                Debug.LogError("Cannot calculate a cutting plane with fewer than 3 coordinates");
                return null;
            }

            //edgePoints.ForEach(p => Debug.Log(p.ToString()));
            var startLeft = GetClosestPoint(edgePoints, intersectionPoints[2]);
            edgePoints.Remove(startLeft);
            var startRight = GetClosestPoint(edgePoints, intersectionPoints[3]);
            edgePoints.Remove(startRight);

            var p1 = startRight; 
            var p2 = edgePoints[1];

            var diff1 = p1 - startLeft;
            var diff2 = p2 - startLeft;
            var (newWidth, newHeight) = GetDimensionsSyncDifferences(ref diff1, ref diff2);

            var width = (int)Math.Round(newWidth, 0); ; // bigger image if angled -  CalculateAngledPlaneLength(p1 - startLeft, newWidth);
            var height = (int)Math.Round(newHeight, 0); ; // bigger image if angled - CalculateAngledPlaneLength(p2 - startLeft, newHeight);

            var xSteps = diff1 / width;
            var ySteps = diff2 / height;
            (xSteps, ySteps) = MinimiseSteps(xSteps, ySteps);

            return new SlicePlaneCoordinates(width, height, startLeft, xSteps, ySteps);
        }

        private Vector3 GetClosestPoint(List<Vector3> edgePoints, Vector3 targetPoint) => edgePoints
            .ToDictionary(p => p, p => Vector3.Distance(p, targetPoint))
            .OrderBy(p => p.Value)
            .First()
            .Key;

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
            //var indexSum = 3;

            var maxWidthIndex = GetIndexOfAbsHigherValue(listWidth);
            var maxHeightIndex = GetIndexOfAbsHigherValue(listHeight);

            var width = listWidth[maxWidthIndex];
            var height = listHeight[maxHeightIndex];

            //var addIndex = (indexSum - maxWidthIndex - maxHeightIndex) % indexSum;
            //var addWidth = listWidth[addIndex];
            //var addHeight = listHeight[addIndex];

            var zeroVector = GetCustomZeroVector(maxWidthIndex);
            if (maxWidthIndex == maxHeightIndex) // cannot use same coordinate for step calculation as a 2d image has 2 coordinates
            {
                listWidth.RemoveAt(maxWidthIndex);
                listHeight.RemoveAt(maxHeightIndex);
                //indexSum = 1;

                maxWidthIndex = GetIndexOfAbsHigherValue(listWidth);
                maxHeightIndex = GetIndexOfAbsHigherValue(listHeight);
                var tempWidth = listWidth[maxWidthIndex];
                var tempHeight = listHeight[maxHeightIndex];

                if (Math.Abs(tempWidth) > Math.Abs(tempHeight))
                {
                    width = tempWidth;
                    diffWidth.x *= zeroVector.x;
                    diffWidth.y *= zeroVector.y;
                    diffWidth.z *= zeroVector.z;
                    //addIndex = indexSum - maxWidthIndex;
                }
                else
                {
                    height = tempHeight;
                    diffHeight.x *= zeroVector.x;
                    diffHeight.y *= zeroVector.y;
                    diffHeight.z *= zeroVector.z;
                    //addIndex = indexSum - maxHeightIndex;
                }

                //addHeight = listHeight[addIndex];
                //addWidth = listWidth[addIndex];
            }

            return (Math.Abs(width), Math.Abs(height));
            //return (Math.Abs(width) + Math.Abs(addWidth), Math.Abs(height) + Math.Abs(addHeight));
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

        public Texture2D CalculateIntersectionPlane(Vector3? alternativeStartPoint = null, InterpolationType interpolationType = InterpolationType.NearestNeighbour)
        {
            if (SlicePlaneCoordinates == null)
            {
                return null;
            }
            var resultImage = new Texture2D(SlicePlaneCoordinates.Width, SlicePlaneCoordinates.Height);

            var startPoint = alternativeStartPoint ?? SlicePlaneCoordinates.StartPoint;
            var currVector1 = startPoint;
            var currVector2 = startPoint;

            for (int w = 0; w < SlicePlaneCoordinates.Width; w++)
            {
                currVector1.x = (int)Math.Round(startPoint.x + w * SlicePlaneCoordinates.XSteps.x, 0);
                currVector1.y = (int)Math.Round(startPoint.y + w * SlicePlaneCoordinates.XSteps.y, 0);
                currVector1.z = (int)Math.Round(startPoint.z + w * SlicePlaneCoordinates.XSteps.z, 0);

                for (int h = 0; h < SlicePlaneCoordinates.Height; h++)
                {
                    currVector2.x = (int)Math.Round(currVector1.x + h * SlicePlaneCoordinates.YSteps.x, 0);
                    currVector2.y = (int)Math.Round(currVector1.y + h * SlicePlaneCoordinates.YSteps.y, 0);
                    currVector2.z = (int)Math.Round(currVector1.z + h * SlicePlaneCoordinates.YSteps.z, 0);

                    var croppedIndex = ValueCropper.CropIntVector(currVector2, _model.GetCountVector());
                    var currBitmap = _model.originalBitmap[croppedIndex.x];

                    Color result;
                    if (interpolationType == InterpolationType.NearestNeighbour)
                    {
                        result = Interpolation.GetNearestNeighbourInterpolation(currBitmap, currBitmap.width, currBitmap.height, croppedIndex.z, croppedIndex.y, false);
                    }
                    else if (interpolationType == InterpolationType.Bilinear)
                    {
                        result = Interpolation.GetBiLinearInterpolatedValue(currBitmap, currBitmap.width, currBitmap.height, croppedIndex.z, croppedIndex.y, false);
                    }
                    else
                    {
                        result = currBitmap.GetPixel(croppedIndex.z, croppedIndex.y);
                    }

                    if (alternativeStartPoint == null) 
                    {
                        result = MakeBlackTransparent(result);
                    }
                    resultImage.SetPixel(w, h, result);
                }
            }

            return resultImage;
        }

        private Color MakeBlackTransparent(Color colour)
        {
            if (colour.r <= ConfigurationConstants.BLACK_TRANSPARENT_THRESHOLD 
                && colour.g <= ConfigurationConstants.BLACK_TRANSPARENT_THRESHOLD 
                && colour.b <= ConfigurationConstants.BLACK_TRANSPARENT_THRESHOLD)
            {
                colour = ColorExtensions.FromArgb(0, colour);
            }
            return colour;
        }

        private IEnumerable<Vector3> CalculateEdgePoints(PlaneFormula planeFormula)
        {
            var edgePoints = new List<Vector3?>();
            var xCount = _model.xCount;
            var yCount = _model.yCount;
            var zCount = _model.zCount;

            edgePoints.Add(planeFormula.GetValidXVectorOnPlane(xCount, 0, 0));
            edgePoints.Add(planeFormula.GetValidXVectorOnPlane(xCount, yCount, 0));
            edgePoints.Add(planeFormula.GetValidXVectorOnPlane(xCount, 0, zCount));
            edgePoints.Add(planeFormula.GetValidXVectorOnPlane(xCount, yCount, zCount));

            edgePoints.Add(planeFormula.GetValidYVectorOnPlane(yCount, 0, 0));
            edgePoints.Add(planeFormula.GetValidYVectorOnPlane(yCount, xCount, 0));
            edgePoints.Add(planeFormula.GetValidYVectorOnPlane(yCount, 0, zCount));
            edgePoints.Add(planeFormula.GetValidYVectorOnPlane(yCount, xCount, zCount));

            edgePoints.Add(planeFormula.GetValidZVectorOnPlane(zCount, 0, 0));
            edgePoints.Add(planeFormula.GetValidZVectorOnPlane(zCount, xCount, 0));
            edgePoints.Add(planeFormula.GetValidZVectorOnPlane(zCount, 0, yCount));
            edgePoints.Add(planeFormula.GetValidZVectorOnPlane(zCount, xCount, yCount));

            var validEdgePoints = edgePoints.Where(p => p is Vector3).Cast<Vector3>();
            return validEdgePoints;
        }

        private Vector3 GetCustomZeroVector(int zeroOnIndex) => new Vector3(zeroOnIndex == 0 ? 0 : 1,
                                                                            zeroOnIndex == 1 ? 0 : 1,
                                                                            zeroOnIndex == 2 ? 0 : 1);

        private int GetIndexOfAbsHigherValue(IList<float> values) => values.IndexOf(values.OrderByDescending(Mathf.Abs).FirstOrDefault());

        private bool IsEdgeValue(float axisCoordinate, float maxValue) => axisCoordinate <= 0 || (axisCoordinate + 1) >= maxValue;

        /// <summary>
        /// Need to find the axis along which the plane can be moved
        /// The startpoint always lays on the max or min of at least two axis
        /// If this is not the case (3 max or min), the plane can only be moved into one direction
        /// </summary>
        public (Texture2D texture, Vector3 startPoint) CalculateNeighbourIntersectionPlane(bool isLeft)
        {
            var stepSize = ConfigurationConstants.NEIGHBOUR_DISTANCE;
            var moveDirection = isLeft ? stepSize : -stepSize;
            var neighbourStartPoint = SlicePlaneCoordinates.StartPoint;

            var isXEdgePoint = IsEdgeValue(SlicePlaneCoordinates.StartPoint.x, _model.xCount);
            var isYEdgePoint = IsEdgeValue(SlicePlaneCoordinates.StartPoint.y, _model.yCount);
            var isZEdgePoint = IsEdgeValue(SlicePlaneCoordinates.StartPoint.z, _model.zCount);

            var isInvalid = false;
            if (isXEdgePoint && isYEdgePoint && isZEdgePoint)
            {
                neighbourStartPoint.x += moveDirection;
                isInvalid = IsInvalidVector(neighbourStartPoint.x, _model.xCount);
            }
            else if (isXEdgePoint && isYEdgePoint)
            {
                neighbourStartPoint.z += moveDirection;
                isInvalid = IsInvalidVector(neighbourStartPoint.z, _model.zCount);
            }
            else if (isXEdgePoint && isZEdgePoint)
            {
                neighbourStartPoint.y += moveDirection;
                isInvalid = IsInvalidVector(neighbourStartPoint.y, _model.yCount);
            }
            else
            {
                neighbourStartPoint.x += moveDirection;
                isInvalid = IsInvalidVector(neighbourStartPoint.x, _model.xCount);
            }
            
            if (isInvalid)
            {
                return (_invalidTexture, SlicePlaneCoordinates.StartPoint);
            }

            ActivateCalculationSound();
            var neighbourSlice = CalculateIntersectionPlane(neighbourStartPoint, InterpolationType.None);
            var fileLocation = FileSaver.SaveBitmapPng(neighbourSlice);
            var sliceTexture = Model.LoadTexture(fileLocation);
            return (sliceTexture, neighbourStartPoint);
        }

        private bool IsInvalidVector(float value, float maxValue) => value < 0 || value >= maxValue;

        public void ActivateCalculationSound()
        {
            if (SlicePlaneCoordinates == null)
            {
                return;
            }
            _audioSource.PlayOneShot(_cameraSound);
        }
    }
}