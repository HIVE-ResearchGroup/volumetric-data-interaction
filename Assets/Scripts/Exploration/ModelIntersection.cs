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
            var yellow = Resources.Load(StringConstants.MaterialYellowHighlighted, typeof(Material)) as Material;
            var black = Resources.Load(StringConstants.MaterialBlack, typeof(Material)) as Material;
            var green = Resources.Load(StringConstants.MaterialGreen, typeof(Material)) as Material;
            var white = Resources.Load(StringConstants.MaterialWhite, typeof(Material)) as Material;

            var intersectionPoints = GetIntersectionPoints();
            var boxCollider = model.GetComponent<BoxCollider>();
            var halfColliderSize = new Vector3(boxCollider.size.x / 2, boxCollider.size.y / 2, boxCollider.size.z / 2);

            var normalisedPositions = new List<Vector3>();
            int i = 0;
            foreach (var p in intersectionPoints)
            {
                var c = CreateDebugPrimitive(p, i == 0 ? yellow : i == 1 ? black : i == 2 ? green : white);
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

        private List<Vector3> GetIntersectionPoints()
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

        private GameObject CreateDebugPrimitive(Vector3 position, Material material, string name = "primitive")
        {
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            primitive.transform.position = position;
            primitive.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            primitive.GetComponent<MeshRenderer>().material = material;
            primitive.name = name;
            return primitive;
        }

        private Vector3 GetNormalisedPosition(Vector3 relativePosition, Vector3 minPosition)
        {
            var x = relativePosition.x + minPosition.x;
            var y = relativePosition.y + minPosition.y;
            var z = relativePosition.z + minPosition.z;

            return new Vector3(x, y, z);
        }
    }
}