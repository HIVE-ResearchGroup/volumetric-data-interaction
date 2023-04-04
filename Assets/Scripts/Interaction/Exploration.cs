using Assets.Scripts.Exploration;
using Assets.Scripts.Helper;
using UnityEngine;

/// <summary>
/// Interactions class has to be attached to gameobject holding the tracked device
/// </summary>
public class Exploration : MonoBehaviour
{
    public GameObject tracker;

    private GameObject host;
    private GameObject cuttingPlane;

    private GameObject modelPrefab;

    private void Start()
    {
        host = GameObject.Find(StringConstants.Host);
        cuttingPlane = GameObject.Find(StringConstants.SectionQuad);

        modelPrefab = Resources.Load(StringConstants.PrefabSectionModel, typeof(GameObject)) as GameObject;
    }

    /// <summary>
    /// e.g. 5 cm before HHD
    /// Do not allow multiple models!
    /// </summary>
    public GameObject CreateModel()
    {       
        var currTrackingPosition = tracker.transform.position;
        currTrackingPosition.z += 5;

        return CreateModel(currTrackingPosition, Quaternion.identity);
    }

    private GameObject CreateModel(Vector3 currPosition, Quaternion rotation)
    {
        var currModel = ModelFinder.FindModelGameObject();
        if (currModel)
        {
            DeleteModel(currModel);
        }

        Debug.Log($"** Create model with name {StringConstants.PrefabSectionModel}.");
        var newModel = Instantiate(modelPrefab, currPosition, rotation);
        if (!cuttingPlane.TryGetComponent(out Slicer slicer))
        {
            slicer = cuttingPlane.AddComponent<Slicer>();
        }

        var listener = newModel.GetComponent<CollisionListener>();
        listener.OnCollisionEnter += _ => slicer.isTouched = true;
        listener.OnCollisionExit += _ => slicer.isTouched = false;
        newModel.name = StringConstants.ModelName;
        return newModel;
    }

    private void DeleteModel(GameObject currModel)
    {
        if (!currModel)
        {
            return;
        }

        if (host.TryGetComponent(out SnapshotInteraction si))
        {
            si.DeleteAllSnapshots();
        }
        Destroy(currModel);
        Debug.Log($"** Model with name {StringConstants.PrefabSectionModel} destroyed.");
    }

    public GameObject ResetModel()
    {
        var currModel = ModelFinder.FindModelGameObject();
        if (!currModel)
        {
            Debug.Log("** There is no model to be reset!");
            return currModel;
        }

        var currPosition = new Vector3(currModel.transform.position.x, currModel.transform.position.y, currModel.transform.position.z);
        var currRotation = new Quaternion(currModel.transform.rotation.x, currModel.transform.rotation.y, currModel.transform.rotation.z, currModel.transform.rotation.w);

        return CreateModel(currPosition, currRotation);
    }
}
