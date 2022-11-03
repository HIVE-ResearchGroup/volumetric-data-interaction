using Assets.Scripts.Helper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Exploration
{
    public class ModelIntersection : MonoBehaviour
    {
        private GameObject plane;
        private GameObject model;
        private Model modelScript;

        public ModelIntersection(GameObject model, GameObject plane)
        {
            this.plane = plane;
            this.model = model;
            this.modelScript = model.GetComponent<Model>();
        }

        public List<Vector3> GetNormalisedIntersectionPosition()
        {
            var intersectionPoints = GetIntersectionPoints();
            var boxCollider = model.GetComponent<BoxCollider>();
            var halfColliderSize = new Vector3(boxCollider.size.x / 2, boxCollider.size.y / 2, boxCollider.size.z / 2);

            var normalisedPositions = new List<Vector3>();
            int i = 0;
            foreach (var p in intersectionPoints)
            {
                var c = VisualDebugger.CreateDebugPrimitive(p, i == 0 ? Color.yellow : i == 1 ? Color.black : i == 2 ? Color.green : Color.white);
                i++;
                c.transform.SetParent(model.transform);
                normalisedPositions.Add(GetNormalisedPosition(c.transform.localPosition, halfColliderSize));
                Destroy(c);
            }

            var positions = CalculatePositionWithinModel(normalisedPositions, boxCollider.size);
            return positions;
        }

        private List<Vector3> GetPlaneMeshVertices()
        {
            var localVertices = plane.GetComponent<MeshFilter>().sharedMesh.vertices;
            var globalVertices = new List<Vector3>();

            foreach (var localPoint in localVertices)
            {
                globalVertices.Add(plane.transform.TransformPoint(localPoint));
            }

            return globalVertices;
        }

        public List<Vector3> GetIntersectionPoints()
        {
            var black = Resources.Load(StringConstants.MaterialBlack, typeof(Material)) as Material;
            var modelCollider = model.GetComponent<Collider>();

            var globalPlaneVertices = GetPlaneMeshVertices();
            var planePosition = plane.transform.position;

            var isTouching = false;
            var touchPoints = new List<Vector3>();
            foreach (var planePoint in globalPlaneVertices)
            {
                isTouching = false;
                var touchPoint = planePoint;

                while (!isTouching && touchPoint != planePosition)
                {
                    touchPoint = Vector3.MoveTowards(touchPoint, planePosition, 0.005f);

                    var hitColliders = Physics.OverlapBox(touchPoint, new Vector3());
                    isTouching = hitColliders.FirstOrDefault(c => c.name == modelCollider.name);
                    //if (isTouching)
                    //{
                    //    CreateDebugPrimitive(touchPoint, black);
                    //}
                }

                touchPoints.Add(touchPoint);
            }

            return touchPoints;
        }
              
        private List<Vector3> CalculatePositionWithinModel(List<Vector3> normalisedContacts, Vector3 size)
        {
            var xMax = modelScript.xCount;
            var yMax = modelScript.yCount;
            var zMax = modelScript.zCount;

            var positions = new List<Vector3>();
            foreach (var contact in normalisedContacts)
            {
                var xRelativePosition = (contact.z / size.z) * xMax;
                var yRelativePosition = (contact.y / size.y) * yMax;
                var zRelativePosition = (contact.x / size.x) * zMax;
                positions.Add(new Vector3(Mathf.Round(xRelativePosition), Mathf.Round(yRelativePosition), Mathf.Round(zRelativePosition)));
            }

            return positions;
        }

        private Vector3 GetNormalisedPosition(Vector3 relativePosition, Vector3 minPosition)
        {
            var x = relativePosition.x + minPosition.x;
            var y = relativePosition.y + minPosition.y;
            var z = relativePosition.z + minPosition.z;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// https://catlikecoding.com/unity/tutorials/procedural-meshes/creating-a-mesh/
        /// </summary>
        public Mesh CreateIntersectingMesh()
        {
            var originalIntersectionPoints = GetIntersectionPoints();
            var intersectionPoints = GetBoundaryIntersections(originalIntersectionPoints, model.GetComponent<BoxCollider>());

            Mesh mesh = new Mesh();
            mesh.vertices = intersectionPoints.ToArray();
            mesh.triangles = new int[6] { 0, 2, 1, 1, 2, 3};
            mesh.normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back , Vector3.back };
            mesh.uv = new Vector2[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };

            return mesh;
        }

        public static Vector3 SetBoundsPoint(Vector3 point, BoxCollider collider)
        {
            var threshold = 0.1f;
            var boundsPoint = collider.ClosestPointOnBounds(point);
            var distance = Vector3.Distance(point, boundsPoint);
            return distance > threshold ? point : boundsPoint;
        }

        public static List<Vector3> GetBoundaryIntersections(List<Vector3> intersectionPoints, BoxCollider modelCollider)
        {
            var p1 = SetBoundsPoint(intersectionPoints[0], modelCollider);
            var p2 = SetBoundsPoint(intersectionPoints[1], modelCollider);
            var p3 = SetBoundsPoint(intersectionPoints[2], modelCollider);
            var p4 = SetBoundsPoint(intersectionPoints[3], modelCollider);

            // vertically
            var v1 = GetMostOutestPointOnBound(p1, p3, modelCollider);
            var v2 = GetMostOutestPointOnBound(p2, p4, modelCollider);
            var v3 = GetMostOutestPointOnBound(p3, p1, modelCollider);
            var v4 = GetMostOutestPointOnBound(p4, p2, modelCollider);

            //horizontally
            var h1 = GetMostOutestPointOnBound(v1, v2, modelCollider);
            var h2 = GetMostOutestPointOnBound(v2, v1, modelCollider);
            var h3 = GetMostOutestPointOnBound(v3, v4, modelCollider);
            var h4 = GetMostOutestPointOnBound(v4, v3, modelCollider);

            return new List<Vector3>() { h1, h2, h3, h4 };
        }

        /// <summary>
        /// Position outside point outside of collider, use two points to create line
        /// move outside point towards original point until collision with collider to find outside border
        /// Beforehand, it was tried to work with ray casting, which was not reliable
        /// See commit f0222339 for obsolete code
        /// </summary>
        private static Vector3 GetMostOutestPointOnBound(Vector3 point, Vector3 referencePoint, BoxCollider collider)
        {
            var direction = point - referencePoint;
            var outsidePoint = point + direction * 20;
            var threshold = 0.01f;

            var maxIterations = 10000;
            var i = 0;
            var distance = 100f;
            while (distance > threshold && i < maxIterations)
            {
                outsidePoint = Vector3.MoveTowards(outsidePoint, point, threshold);
                distance = Vector3.Distance(outsidePoint, collider.ClosestPointOnBounds(outsidePoint));
                i++;
            }

            var result = i == maxIterations ? point : outsidePoint;
            return SetBoundsPoint(result, collider);
        }
    }
}