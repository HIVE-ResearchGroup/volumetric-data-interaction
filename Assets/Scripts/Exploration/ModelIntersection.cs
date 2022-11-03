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

        public Mesh CreateIntersectingMesh()
        {
            var originalIntersectionPoints = GetIntersectionPoints();
            var intersectionPoints = GetBoundaryIntersections(originalIntersectionPoints, model.GetComponent<BoxCollider>());

            Mesh mesh = new Mesh();
            mesh.vertices = intersectionPoints.ToArray();
            mesh.triangles = new int[6] { 1, 3, 0, 3, 2, 0 };
            // still not turned .. { 0, 1, 2, 1, 3, 2 }; // nice but need 90 degrees clockwise { 1, 3, 0, 3, 2, 0 }; 
            mesh.normals = new Vector3[4] { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward };
            mesh.uv = new Vector2[4] { new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1) };

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
            var p1 = intersectionPoints[0]; 
            var p2 = intersectionPoints[1];
            var p3 = intersectionPoints[2]; 
            var p4 = intersectionPoints[3];

            // temporary horizontal alignment
            var t12 = SetBoundsPoint(GetEdgePoint(p1, p2, modelCollider), modelCollider);
            var t21 = SetBoundsPoint(GetEdgePoint(p2, p1, modelCollider), modelCollider);
            var t34 = SetBoundsPoint(GetEdgePoint(p3, p4, modelCollider), modelCollider);
            var t43 = SetBoundsPoint(GetEdgePoint(p4, p3, modelCollider), modelCollider);

            //// temporary vertical alignment
            var t13 = SetBoundsPoint(GetEdgePoint(t12, t34, modelCollider), modelCollider);
            var t31 = SetBoundsPoint(GetEdgePoint(t34, t12, modelCollider), modelCollider);
            var t24 = SetBoundsPoint(GetEdgePoint(t21, t43, modelCollider), modelCollider);
            var t42 = SetBoundsPoint(GetEdgePoint(t43, t21, modelCollider), modelCollider);

            var r1 = GetMostOutestPointOnBound(t13, t31, modelCollider);
            var r2 = GetMostOutestPointOnBound(t31, t13, modelCollider);
            var r3 = GetMostOutestPointOnBound(t24, t42, modelCollider);
            var r4 = GetMostOutestPointOnBound(t42, t24, modelCollider);

            var result = new List<Vector3>() { r1, r2, r3, r4 };
            foreach (var p in result)
            {
                VisualDebugger.CreateDebugPrimitive(p, Color.red);
            }
            return result;
        }

        /// <summary>
        /// Beforehand, it was tried to work with ray casting, which was not very successful
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
                VisualDebugger.CreateDebugPrimitive(outsidePoint, Color.green);
                distance = Vector3.Distance(outsidePoint, collider.ClosestPointOnBounds(outsidePoint));
                i++;
            }

            return i == maxIterations ? point : outsidePoint;
        }

        /// <summary>
        /// Move point away from reference point
        /// Use new position to create a ray pointing back to its origin
        /// Points are just inside their collider, moving them outside allows to use Raycasting to find border of collider
        /// </summary>
        private static Vector3 GetEdgePoint(Vector3 point, Vector3 referencePoint, BoxCollider collider)
        {
            var direction = referencePoint - point;
            var outside = point + direction * 20;

            var hasPoint = false;
            var boundpoint = FindBoundPointWithRaycast(point, outside, collider, out hasPoint);
            if (hasPoint)
            {
                return boundpoint;
            }
                                   
            var i = 1;
            var step = 0.05f;
            var outsideOffset = new Vector3(step, step, step);
            while (i < 10)
            {
                outside += outsideOffset;
                boundpoint = FindBoundPointWithRaycast(point, outside , collider, out hasPoint);
                if (hasPoint)
                {
                    return boundpoint;
                }
                i++;
            }

            Debug.DrawRay(outside, point - outside, Color.red, 200);
            Debug.Log("Edge point or cut could not be calculated.");
            return SetBoundsPoint(point, collider);
        }

        // outside in, need to start from outsidepoint!
        private static Vector3 FindBoundPointWithRaycast(Vector3 originalPoint, Vector3 referencePoint, BoxCollider collider, out bool hasSucceeded)
        {
            var direction = originalPoint - referencePoint;
            var outsideInRay = new Ray(referencePoint, direction);
            RaycastHit hit;
            if (collider.Raycast(outsideInRay, out hit, 25.0f))
            {
                hasSucceeded = true;
                return hit.point;
            }
            Debug.DrawRay(referencePoint, direction, Color.white, 200);
            hasSucceeded = false;
            return referencePoint;
        }
    }
}