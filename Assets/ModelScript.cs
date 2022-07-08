using Assets.Scripts.Exploration;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using UnityEngine;

public class ModelScript : MonoBehaviour
{
    public GameObject plane;

    private Model model;
    
    private float intersectionThreshold = 5f;
    private float intersectionTimer = 5f;

    void Start()
    {
        model = new Model();
    }

    private void OnCollisionStay(Collision collision)
    {       
        if (intersectionTimer <= intersectionThreshold)
        {
            intersectionTimer += Time.deltaTime;
        }
        else if (Input.GetKeyDown(KeyCode.C) && intersectionTimer > intersectionThreshold)
        {
            intersectionTimer = 0;             
            
            var yellow = Resources.Load(StringConstants.MaterialYellowHighlighted, typeof(Material)) as Material;
            var green = Resources.Load(StringConstants.MaterialGreen, typeof(Material)) as Material;
            var black = Resources.Load(StringConstants.MaterialBlack, typeof(Material)) as Material;

            var modelCollider = gameObject.GetComponent<Collider>();
            var planeCollider = plane.GetComponent<Collider>();

            var intersectionPoints = GetIntersectionPoints();

            var normalisedPositions = new List<Vector3>();
            foreach (var p in intersectionPoints)
            {
                var c = CreateDebugPrimitive(p, yellow);
                c.transform.SetParent(gameObject.transform);
                normalisedPositions.Add(GetNormalisedPosition(c.transform.position, modelCollider.bounds.min));
                Destroy(c);
            }

            var positions = CalculatePositionWithinModel(normalisedPositions, modelCollider.bounds.size);

            var intersection = model.CalculateCuttingplane(positions);
            var fileName = $"Testimg";

            var fileLocation = Path.Combine(ConfigurationConstants.IMAGES_FOLDER_PATH, fileName + ".bmp");
            intersection.Save(fileLocation, ImageFormat.Bmp);
        }
    }

    private List<Vector3> GetMeshVertices(GameObject gameObject)
    {
        var localVertices = gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
        var globalVertices = new List<Vector3>();

        foreach (var localPoint in localVertices)
        {
            globalVertices.Add(gameObject.transform.TransformPoint(localPoint));
        }

        return globalVertices;
    }

    private List<Vector3> GetIntersectionPoints()
    {
        var black = Resources.Load(StringConstants.MaterialBlack, typeof(Material)) as Material;
        var modelCollider = gameObject.GetComponent<Collider>();

        var globalPlaneVertices = GetMeshVertices(plane);
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
                if (isTouching)
                {
                    CreateDebugPrimitive(touchPoint, black);
                }
            }

            touchPoints.Add(touchPoint);
        }

        return touchPoints;
    }

    private List<Vector3> CalculatePositionWithinModel(List<Vector3> normalisedContacts, Vector3 size)
    {
        var xMax = model.xCount;
        var yMax = model.yCount;
        var zMax = model.zCount;

        var positions = new List<Vector3>();
        foreach (var contact in normalisedContacts)
        {
            var xRelativePosition = (contact.x / size.x) * xMax;
            var yRelativePosition = (contact.y / size.y) * yMax;
            var zRelativePosition = (contact.z / size.z) * zMax;
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
        var x = relativePosition.x + Mathf.Abs(minPosition.x);
        var y = relativePosition.y + Mathf.Abs(minPosition.y);
        var z = relativePosition.z + Mathf.Abs(minPosition.z);

        return new Vector3(x, y, z);
    }

    #region Calculation using TriggerEnter and Collider - kind of legacy, not working for all rotations

        // get points which are not on the plane and not on bounds but within model
        //var normalisedPositions = new List<Vector3>();
        //int i = 0;
        //foreach (var node in contacts)
        //{
        //    CreateDebugPrimitive(node.point, yellow, "y " + i);

        //var point = modelCollider.ClosestPointOnBounds(node.point);
        //var child = CreateDebugPrimitive(point, green, "g " + i);
        //child.transform.SetParent(gameObject.transform);

        //    normalisedPositions.Add(GetNormalisedPosition(child.transform.position, modelCollider.bounds.min));

        //    i++;
        //}

private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            var yellow = Resources.Load(StringConstants.MaterialYellowHighlighted, typeof(Material)) as Material;
            var black = Resources.Load(StringConstants.MaterialBlack, typeof(Material)) as Material;
            var green = Resources.Load(StringConstants.MaterialGreen, typeof(Material)) as Material;

            var localPlaneVertices = plane.GetComponent<MeshFilter>().sharedMesh.vertices;
            var globalPlaneVertices = new Vector3[localPlaneVertices.Length];
            for (int i = 0; i < localPlaneVertices.Length; i++)
            {
                globalPlaneVertices[i] = plane.transform.TransformPoint(localPlaneVertices[i]);
                CreateDebugPrimitive(globalPlaneVertices[i], black, "s " + i);
            }

            var closestPoints = new Vector3[globalPlaneVertices.Length];
            for (int i = 0; i < globalPlaneVertices.Length; i++)
            {
                var node = gameObject.GetComponent<Collider>().ClosestPointOnBounds(globalPlaneVertices[i]);
                CreateDebugPrimitive(node, yellow, "c " + i);

                // box does not catch all and mesh neither, together they do
                var meshCollider = plane.GetComponent<MeshCollider>();
                var boxCollider = plane.GetComponent<BoxCollider>();

                var meshCollision = GetPlaneIntersectionVector3(meshCollider, node);
                var boxCollision = GetPlaneIntersectionVector3(boxCollider, node);

                var newNode = meshCollision.HasCollision ? meshCollision.Vector : boxCollision.Vector;
                CreateDebugPrimitive(newNode, green, "n" + i);
            }
        }
    }

    // need closest point on boundry of model
    // need closest point on plane...
    private (Vector3 Vector, bool HasCollision) GetPlaneIntersectionVector3(Collider collider, Vector3 startVector)
    {
        var upDown = GetPlaneDistance(collider, startVector, Vector3.up, Vector3.down);

        if (upDown.HasPlaneCollision)
        {
            return (new Vector3(startVector.x, startVector.y + upDown.distance, startVector.z), true);
        }

        var leftRight = GetPlaneDistance(collider, startVector, Vector3.left, Vector3.right);
        if (leftRight.HasPlaneCollision)
        {
            return (new Vector3(startVector.x, startVector.y, startVector.z + leftRight.distance), true);
        }

        var forBackward = GetPlaneDistance(collider, startVector, Vector3.forward, Vector3.back);
        if (forBackward.HasPlaneCollision)
        {
            return (new Vector3(startVector.x + forBackward.distance, startVector.y, startVector.z), true);
        }

        return (startVector, false);
    }


    private (bool HasPlaneCollision, float distance) GetPlaneDistance(Collider collider, Vector3 startNode, Vector3 direction1, Vector3 direction2)
    {
        RaycastHit hitUp, hitDown;

        var up = collider.Raycast(new Ray(startNode, Vector3.up), out hitUp, 1000f);
        var down = collider.Raycast(new Ray(startNode, Vector3.down), out hitDown, 1000f);

        // vllt zu erst checken ob bereits eine collision besteht? 
        // sonst dann erst links rechts && vorne hinten checken
        if (!up && !down)
        {
            return (false, 0f);
        }

        var distance = up ? hitUp.distance : -1 * hitDown.distance;
        return (true, distance);
    }
    #endregion
}
