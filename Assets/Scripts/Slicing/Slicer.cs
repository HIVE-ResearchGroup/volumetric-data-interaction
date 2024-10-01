#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Selection;
using Snapshots;
using UnityEngine;
using Plane = UnityEngine.Plane;

namespace Slicing
{
    public class Slicer : MonoBehaviour
    {
        //[SerializeField]
        //private CutQuad cutQuadPrefab = null!;
        
        [SerializeField]
        private Material materialTemporarySlice = null!;
        
        //[SerializeField]
        //private Material materialWhite = null!;
        
        //[SerializeField]
        //private Material slicedObjectSecondaryMaterial = null!;
        
        [SerializeField]
        private Shader materialShader = null!;
        
        // private bool isTouched;
        //
        // private void OnTriggerEnter(Collider other)
        // {
        //     if (!other.CompareTag(Tags.Model))
        //     {
        //         return;
        //     }
        //
        //     isTouched = true;
        // }
        //
        // private void OnTriggerExit(Collider other)
        // {
        //     if (!other.CompareTag(Tags.Model))
        //     {
        //         return;
        //     }
        //
        //     isTouched = false;
        // }
        //
        // public void Slice()
        // {
        //     if (!isTouched)
        //     {
        //         return;
        //     }
        //     
        //     Debug.Log("Slicing");
        //
        //     var cachedTransform = transform;
        //     var model = ModelManager.Instance.CurrentModel;
        //     var modelGo = model.gameObject;
        //     
        //     var slicedObject = modelGo.Slice(cachedTransform.position, cachedTransform.forward);
        //     if (slicedObject == null)
        //     {
        //         Debug.LogError("Nothing sliced");
        //         return;
        //     }
        //     AudioManager.Instance.PlayCameraSound();
        //
        //     transform.GetPositionAndRotation(out var position, out var rotation);
        //     var points = GetIntersectionPointsFromWorld(model, position, rotation);
        //     if (points == null)
        //     {
        //         Debug.LogWarning("Intersection image can't be calculated!");
        //         return;
        //     }
        //     
        //     var dimensions = GetTextureDimension(model, points);
        //     if (dimensions == null)
        //     {
        //         Debug.LogWarning("SliceCoords can't be calculated!");
        //         return;
        //     }
        //     var texData = CreateSliceTextureData(model, dimensions, points);
        //     var texture = CreateSliceTexture(dimensions, texData);
        //     var mesh = CreateMesh(model, points);
        //     
        //     var transparentMaterial = MaterialTools.CreateTransparentMaterial();
        //     transparentMaterial.name = "SliceMaterial";
        //     transparentMaterial.mainTexture = texture;
        //
        //     Debug.Log($"Sliced gameobject \"{model.name}\"");
        //     var lowerHull = slicedObject.CreateUpperHull(modelGo, slicedObjectSecondaryMaterial);
        //     model.UpdateModel(lowerHull, gameObject);
        //     Destroy(lowerHull);
        //     SetCuttingActive(true);
        //
        //     var quad = Instantiate(cutQuadPrefab, model.transform, true);
        //     // stop Z-fighting by moving slightly up
        //     var pos = quad.transform.position;
        //     pos += transform.back().normalized * 0.0001f;
        //     quad.transform.position = pos;
        //     quad.name = "cut";
        //     quad.Mesh = mesh;
        //     quad.Material = transparentMaterial;
        // }

        public Snapshot? CreateSnapshot()
        {
            transform.GetPositionAndRotation(out var position, out var rotation);
            return SnapshotManager.Instance.CreateSnapshot(0, position, rotation);
        }
        
        public void SetCuttingActive(bool active)
        {
            var model = ModelManager.Instance.CurrentModel;

            model.SetCuttingPlaneActive(active);

            if (active)
            {
                model.SetModelMaterial(materialTemporarySlice, materialShader);
            }
            else
            {
                if (!model.TryGetComponent<Selectable>(out var selectable))
                {
                    return;
                }
                // trigger a highlight refresh
                selectable.RerunHighlightEvent();
            }
        }
        
        public static IntersectionPoints? GetIntersectionPointsFromWorld(Model.Model model, Vector3 position, Quaternion rotation)
        {
            return GetIntersectionPointsFromLocal(model, model.transform.InverseTransformPoint(position), model.transform.InverseTransformVector(rotation * Vector3.back));
        }
        
        public static IntersectionPoints? GetIntersectionPointsFromLocal(Model.Model model, Vector3 localPosition, Vector3 normalVector)
        {
            //// connecting edges
            //Debug.DrawLine(model.BottomFrontLeftCorner, model.BottomBackLeftCorner, Color.black);
            //Debug.DrawLine(model.BottomFrontRightCorner, model.BottomBackRightCorner, Color.black);
            //Debug.DrawLine(model.TopFrontLeftCorner, model.TopBackLeftCorner, Color.black);
            //Debug.DrawLine(model.TopFrontRightCorner, model.TopBackRightCorner, Color.black);

            //// backside edges
            //Debug.DrawLine(model.BottomBackLeftCorner, model.TopBackLeftCorner, Color.black);
            //Debug.DrawLine(model.BottomBackRightCorner, model.TopBackRightCorner, Color.black);
            //Debug.DrawLine(model.BottomBackLeftCorner, model.BottomBackRightCorner, Color.black);
            //Debug.DrawLine(model.TopBackLeftCorner, model.TopBackRightCorner, Color.black);

            //// frontside edges
            //Debug.DrawLine(model.BottomFrontLeftCorner, model.TopFrontLeftCorner, Color.black);
            //Debug.DrawLine(model.BottomFrontRightCorner, model.TopFrontRightCorner, Color.black);
            //Debug.DrawLine(model.BottomFrontLeftCorner, model.BottomFrontRightCorner, Color.black);
            //Debug.DrawLine(model.TopFrontLeftCorner, model.TopFrontRightCorner, Color.black);
            
            if (model.Size == Vector3.zero)
            {
                Debug.LogError("Check if the current model is properly initialized! Is the image path set?");
                return null;
            }

            // this is the normal of the slicer
            var plane = new Plane(normalVector, localPosition);

            // we only take the plane if it faces up (normal points down), else we just flip it
            // the rotation section is CORRECT
            // we need to check normal vector again, because slicerRotation on its own could point up
            var normal = normalVector;
            if (normal.y > 0)
            {
                normal *= -1;
            }

            var rotation = Quaternion.LookRotation(normal);
            var slicerLeft = rotation * Vector3.left;

            var points = GetIntersectionPoints_internal(model, plane).ToList();
            if (points.Count < 3)
            {
                Debug.LogError("Cannot create proper intersection with less than 3 points!");
                return null;
            }

            //var colorArray = new[]
            //{
            //    Color.red,
            //    Color.yellow,
            //    Color.green,
            //    Color.blue,
            //    Color.cyan,
            //    Color.magenta
            //};
            
            //for (var i = 0; i < points.Count; i++)
            //{
            //    Debug.DrawLine(points[i], points[(i + 1) % points.Count], colorArray[i]);
            //}
            
            points = ConvertTo4Points(rotation, points).ToList();

            // points from here are always 4

            var heightSortedPoints = points.OrderByDescending(p => p.y).ToList();

            var topPoints = heightSortedPoints.Take(2).ToList();
            var bottomPoints = heightSortedPoints.Reverse<Vector3>().Take(2).ToList();

            plane = new Plane(slicerLeft, 0);
            plane.Raycast(new Ray(topPoints[0], slicerLeft), out var distance0);
            plane.Raycast(new Ray(topPoints[1], slicerLeft), out var distance1);
            Vector3 topLeft;
            Vector3 topRight;
            if (distance0 < distance1)
            {
                topLeft = topPoints[0];
                topRight = topPoints[1];
            }
            else
            {
                topLeft = topPoints[1];
                topRight = topPoints[0];
            }

            plane.Raycast(new Ray(bottomPoints[0], slicerLeft), out distance0);
            plane.Raycast(new Ray(bottomPoints[1], slicerLeft), out distance1);
            Vector3 bottomLeft;
            Vector3 bottomRight;
            if (distance0 < distance1)
            {
                bottomLeft = bottomPoints[0];
                bottomRight = bottomPoints[1];
            }
            else
            {
                bottomLeft = bottomPoints[1];
                bottomRight = bottomPoints[0];
            }

            //Debug.DrawLine(topLeft, bottomLeft, Color.blue);
            //Debug.DrawLine(bottomLeft, bottomRight, Color.green);
            //Debug.DrawLine(bottomRight, topRight, Color.yellow);
            //Debug.DrawLine(topRight, topLeft, Color.red);

            return new IntersectionPoints
            {
                UpperLeft = topLeft,
                LowerLeft = bottomLeft,
                LowerRight = bottomRight,
                UpperRight = topRight
            };
        }
        
        public static Dimensions? GetTextureDimension(Model.Model model, IntersectionPoints points)
        {
            var ul = points.UpperLeft;
            var ll = points.LowerLeft;
            var lr = points.LowerRight;
            
            var diffHeight = ul - ll;
            var diffXZ = lr - ll;

            // this is for calculating steps for height
            var ySteps = Mathf.RoundToInt(diffHeight.y / model.StepSize.y);    // Math.Abs is not needed, ySteps is ALWAYS from bottom to top

            var forwardStepsX = Mathf.RoundToInt(diffHeight.x / model.StepSize.x);
            var forwardStepsZ = Mathf.RoundToInt(diffHeight.z / model.StepSize.z);

            // this is for calculating steps for width
            var xSteps = Mathf.RoundToInt(diffXZ.x / model.StepSize.x);
            var zSteps = Mathf.RoundToInt(diffXZ.z / model.StepSize.z);

            var width = xSteps;
            if (Math.Abs(zSteps) > Math.Abs(width))
            {
                width = zSteps;
            }

            var height = ySteps;
            if (Math.Abs(forwardStepsX) > Math.Abs(height))
            {
                height = forwardStepsX;
            }
            if (Math.Abs(forwardStepsZ) > Math.Abs(height))
            {
                height = forwardStepsZ;
            }

            if (width == 0 || height == 0)
            {
                return null;
            }

            return new Dimensions
            {
                Width = width,
                Height = height
            };
        }

        public static Color32[] CreateSliceTextureData(Model.Model model, Dimensions dimensions, IntersectionPoints points)
        {
            var width = Math.Abs(dimensions.Width);
            var height = Math.Abs(dimensions.Height);

            var data = new Color32[width * height];

            var start = points.LowerLeft;
            var xStep = (points.LowerRight - points.LowerLeft) / width;
            var yStep = (points.UpperLeft - points.LowerLeft) / height;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var position = start + xStep * x + yStep * y;

                    var index = model.LocalPositionToIndex(position);

                    // this line is essential to flip the image in the right way
                    // because flipping the UV and horizontal and vertical axis will adjust it based on the BACKSIDE of the mesh
                    // by flipping the index we can flip it from the BACK to the FRONT of the mesh, finally matching the model
                    index.x = model.XCount - index.x;

                    // get image at index and then the pixel
                    var pixel = model.GetPixel32(index);
                    data[x + y * width] = pixel;
                }
            }

            return data;
        }

        public static Texture2D CreateSliceTexture(Dimensions dimensions, Color32[] data)
        {
            var width = Math.Abs(dimensions.Width);
            var height = Math.Abs(dimensions.Height);

            var resultImage = new Texture2D(width, height);
            resultImage.SetPixels32(data);
            resultImage.Apply();
            return resultImage;
        }

        public static Mesh CreateMesh(Model.Model model, IntersectionPoints points)
        {
            // convert to world coordinates
            var worldPoints = new IntersectionPoints
            {
                UpperLeft = model.transform.TransformPoint(points.UpperLeft),
                LowerLeft = model.transform.TransformPoint(points.LowerLeft),
                LowerRight = model.transform.TransformPoint(points.LowerRight),
                UpperRight = model.transform.TransformPoint(points.UpperRight)
            };

            var arr = new Vector3[4];
            arr[0] = worldPoints.UpperLeft;
            arr[1] = worldPoints.LowerLeft;
            arr[2] = worldPoints.LowerRight;
            arr[3] = worldPoints.UpperRight;
            
            return new Mesh
            {
                vertices = arr,
                triangles = new int[] { 0, 2, 1, 0, 3, 2,
                    0, 1, 2, 0, 2, 3 },
                normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back },
                uv = new Vector2[] { Vector2.up, Vector2.zero, Vector2.right, Vector2.one }
            };
        }
        
        /// <summary>
        /// Tests all edges for cuts and returns them.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        private static IEnumerable<Vector3> GetIntersectionPoints_internal(Model.Model model, Plane plane)
        {
            var list = new List<Vector3>(6);
            var size = model.Size;

            // test Z axis (front - back)
            var ray = new Ray(model.TopFrontLeftCorner, Vector3.forward);
            if (HitCheck(out var point, plane, ray, size.z))
            {
                list.Add(point);
            }

            ray = new Ray(model.TopFrontRightCorner, Vector3.forward);
            if (HitCheck(out point, plane, ray, size.z))
            {
                list.Add(point);
            }

            ray = new Ray(model.BottomFrontLeftCorner, Vector3.forward);
            if (HitCheck(out point, plane, ray, size.z))
            {
                list.Add(point);
            }

            ray = new Ray(model.BottomFrontRightCorner, Vector3.forward);
            if (HitCheck(out point, plane, ray, size.z))
            {
                list.Add(point);
            }

            // test Y axis (top - bottom)
            ray = new Ray(model.TopBackLeftCorner, Vector3.down);
            if (HitCheck(out point, plane, ray, size.y))
            {
                list.Add(point);
            }

            ray = new Ray(model.TopFrontLeftCorner, Vector3.down);
            if (HitCheck(out point, plane, ray, size.y))
            {
                list.Add(point);
            }

            ray = new Ray(model.TopFrontRightCorner, Vector3.down);
            if (HitCheck(out point, plane, ray, size.y))
            {
                list.Add(point);
            }

            ray = new Ray(model.TopBackRightCorner, Vector3.down);
            if (HitCheck(out point, plane, ray, size.y))
            {
                list.Add(point);
            }

            // test X axis (left - right)
            ray = new Ray(model.TopFrontLeftCorner, Vector3.right);
            if (HitCheck(out point, plane, ray, size.x))
            {
                list.Add(point);
            }

            ray = new Ray(model.BottomFrontLeftCorner, Vector3.right);
            if (HitCheck(out point, plane, ray, size.x))
            {
                list.Add(point);
            }

            ray = new Ray(model.TopBackLeftCorner, Vector3.right);
            if (HitCheck(out point, plane, ray, size.x))
            {
                list.Add(point);
            }

            ray = new Ray(model.BottomBackLeftCorner, Vector3.right);
            if (HitCheck(out point, plane, ray, size.x))
            {
                list.Add(point);
            }

            return list;

            static bool HitCheck(out Vector3 hitPoint, Plane plane, Ray ray, float maxDistance)
            {
                var result = plane.Raycast(ray, out var distance);
                if (!(!result && distance == 0) &&  // false AND 0 means plane and raycast are parallel
                    ((distance >= 0 && distance <= maxDistance) ||
                     (distance < 0 && distance >= maxDistance)))
                {
                    hitPoint = ray.GetPoint(distance);
                    return true;
                }
                hitPoint = Vector3.zero;
                return false;
            }
        }

        private static IEnumerable<Vector3> ConvertTo4Points(Quaternion planeRotation, IReadOnlyList<Vector3> points)
        {
            var left = planeRotation * Vector3.left;
            var right = planeRotation * Vector3.right;

            var plane = new Plane(right, points[0]);
            var sortedPoints = points.Select(p =>
                {
                    plane.Raycast(new Ray(p, left), out var d);
                    return (p, d);
                })
                .OrderBy(p => p.d)
                .Select(p => p.p)
                .ToList();

            var leftPoint = sortedPoints.First();
            var rightPoint = sortedPoints.Last();

            sortedPoints = points.OrderBy(p => p.y).ToList();
            var topPoint = sortedPoints.Last();
            var bottomPoint = sortedPoints.First();

            plane.SetNormalAndPosition(left, leftPoint);
            plane.Raycast(new Ray(topPoint, left), out var distance);
            yield return topPoint + distance * left;

            plane.Raycast(new Ray(bottomPoint, left), out distance);
            yield return bottomPoint + distance * left;

            plane.SetNormalAndPosition(right, rightPoint);
            plane.Raycast(new Ray(bottomPoint, right), out distance);
            yield return bottomPoint + distance * right;

            plane.Raycast(new Ray(topPoint, right), out distance);
            yield return topPoint + distance * right;
        }
    }
}