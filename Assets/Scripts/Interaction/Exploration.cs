using Assets.Scripts.Exploration;
using UnityEngine;

/// <summary>
/// Interactions class has to be attached to gameobject holding the tracked device
/// </summary>
public class Exploration : MonoBehaviour
{
    public GameObject tracker;

    /*public Exploration(GameObject tracker)
    {
        this.tracker = tracker;
    }*/

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
        var currModel = FindCurrentModel();
        if (currModel)
        {
            DeleteModel(currModel);
        }

        var model = Resources.Load(StringConstants.PrefabSectionModel, typeof(GameObject)) as GameObject;
        Debug.Log($"** Create model with name {StringConstants.PrefabSectionModel}.");
        var newModel = Instantiate(model, currPosition, rotation);
        var cuttingPlane = GameObject.Find(StringConstants.SectionQuad);
        if (!cuttingPlane.TryGetComponent(out Slicer slicer))
        {
            slicer = cuttingPlane.AddComponent<Slicer>();
        }

        newModel.GetComponent<SliceListener>().slicer = slicer;
        newModel.name = StringConstants.ModelName;
        return newModel;
    }

    private GameObject FindCurrentModel()
    {
        var currModel = Model.GetModelGameObject();
        if (currModel == null)
        {
            Debug.Log($"** No model with name {StringConstants.Model} found.");
        }
        return currModel;
    }

    private void DeleteModel(GameObject currModel)
    {
        if (!currModel)
        {
            return;
        }

        GameObject.Find(StringConstants.Host).GetComponent<SnapshotInteraction>()?.DeleteAllSnapshots();
        Destroy(currModel);
        Debug.Log($"** Model with name {StringConstants.PrefabSectionModel} destroyed.");
    }

    public GameObject ResetModel()
    {
        var currModel = FindCurrentModel();
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
