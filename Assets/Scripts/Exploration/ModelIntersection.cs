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
            mesh.triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
            mesh.normals = new Vector3[4] { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward };
            mesh.uv = new Vector2[4] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };

            return mesh;
        }

        public static List<Vector3> GetBoundaryIntersections(List<Vector3> intersectionPoints, BoxCollider modelCollider)
        {
            var p1 = intersectionPoints[0]; 
            var p2 = intersectionPoints[1];
            var p3 = intersectionPoints[2]; 
            var p4 = intersectionPoints[3];

            // temporary horizontal alignment
            var t12 = GetEdgePoint(p1, p2, modelCollider);
            var t21 = GetEdgePoint(p2, p1, modelCollider);
            var t34 = GetEdgePoint(p3, p4, modelCollider);
            var t43 = GetEdgePoint(p4, p3, modelCollider);

            // temporary vertical alignment
            var t13 = GetEdgePoint(t12, t34, modelCollider);
            var t31 = GetEdgePoint(t34, t12, modelCollider);
            var t24 = GetEdgePoint(t21, t43, modelCollider);
            var t42 = GetEdgePoint(t43, t21, modelCollider);

            // horizontal alignment
            var r12 = GetEdgePoint(t13, t24, modelCollider);
            var r21 = GetEdgePoint(t24, t13, modelCollider);
            var r34 = GetEdgePoint(t31, t42, modelCollider);
            var r43 = GetEdgePoint(t42, t31, modelCollider);
            //VisualDebugger.CreateDebugPrimitive(r12, Color.yellow);
            //VisualDebugger.CreateDebugPrimitive(r21, Color.yellow);
            //VisualDebugger.CreateDebugPrimitive(r34, Color.yellow);
            //VisualDebugger.CreateDebugPrimitive(r43, Color.yellow);

            var r13 = GetEdgePoint(r12, r34, modelCollider);
            var r31 = GetEdgePoint(r34, r12, modelCollider);
            var r24 = GetEdgePoint(r21, r43, modelCollider);
            var r42 = GetEdgePoint(r43, r21, modelCollider);
            //VisualDebugger.CreateDebugPrimitive(r13, Color.red);
            //VisualDebugger.CreateDebugPrimitive(r31, Color.red);
            //VisualDebugger.CreateDebugPrimitive(r24, Color.red);
            //VisualDebugger.CreateDebugPrimitive(r42, Color.red);

            var result = new List<Vector3>() { r13, r31, r24, r42 };
            return result;
        }

        /// <summary>
        /// Move point away from reference point
        /// Use new position to create a ray pointing back to its origin
        /// Points are just inside their collider, moving them outside allows to use Raycasting to find border of collider
        /// </summary>
        private static Vector3 GetEdgePoint(Vector3 point, Vector3 referencePoint, Collider collider)
        {
            var direction = referencePoint - point;
            var outside = point + direction * 20;
            var outsideInRay = new Ray(outside, point - outside);

            RaycastHit hit;
            if (collider.Raycast(outsideInRay, out hit, 20.0f))
            {
                Debug.DrawRay(outside, point - outside, Color.white, 10);
                return hit.point;
            }

            var i = 1;
            var step = 0.01f;
            while (i < 10)
            {
                var currStep = i % 2 == 1 ? step * i : step * -i;
                outside += new Vector3(currStep, currStep, currStep);

                if (collider.Raycast(outsideInRay, out hit, 20.0f))
                {
                    return hit.point;
                }
                i++;
            }

            Debug.LogError("Edge point or cut could not be calculated.");
            return referencePoint;
        }
    }
}