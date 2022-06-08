using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interactions class has to be attached to gameobject holding the tracked device
/// </summary>
public class AnalysisInteraction : MonoBehaviour
{
    private GameObject tracker;
    private GameObject sectionQuad;
    private GameObject model;

    public AnalysisInteraction(GameObject tracker)
    {
        this.tracker = tracker;
        model = Resources.Load(StringConstants.PrefabSectionModel, typeof(GameObject)) as GameObject;
        sectionQuad = Resources.Load(StringConstants.PrefabSectionQuad, typeof(GameObject)) as GameObject;
    }


    /// <summary>
    /// Create model at specified position
    /// e.g. 5 cm before HHD
    /// Do not allow multiple models!
    /// Set dummy cuttingPlane for cuttingScript to avoid error
    /// </summary>
    public void CreateModel()
    {
        var currModel = FindCurrentModel();

        if (currModel)
        {
            DeleteModel();
        }

        var currTrackingPosition = tracker.transform.position;
        var currTrackingRotation = tracker.transform.rotation; //todo - adjust if necesssary

        currModel = Instantiate(model, new Vector3(currTrackingPosition.x, currTrackingPosition.y, currTrackingPosition.z + 5), Quaternion.identity);
    }

    private GameObject FindCurrentModel()
    {
        var currModel = GameObject.Find(model.name) ?? GameObject.Find($"{model.name}{StringConstants.Clone}");
        if (currModel == null)
        {
            Debug.Log($"** No model with name {model.name} found.");
        }
        return currModel;
    }

    /// <summary>
    /// Remove existing cutting planes first due to connection in model
    /// Remove existing model
    /// </summary>
    public void DeleteModel()
    {
        Debug.Log("Delete model");
        var currModel = FindCurrentModel();

        if (currModel)
        {
            DeleteAllCuttingPlanes();
            DeleteAllSnapshots();
            Destroy(currModel);
            Debug.Log($"** Model with name {model.name} destroyed.");
        }
    }

    /// <summary>
    /// Delete existing Model (is existing)
    /// Delete existing cutting planes (is existing)
    /// Create new Model (at position of previous model)
    /// </summary>
    public void ResetModel()
    {
        Debug.Log("Reset model");

        var currentModel = FindCurrentModel();
        if (!currentModel)
        {
            Debug.Log("** There is no model to be reset!");
            return;
        }

        var currPosition = new Vector3(currentModel.transform.position.x, currentModel.transform.position.y, currentModel.transform.position.z);
        var currRotation = new Quaternion(currentModel.transform.rotation.x, currentModel.transform.rotation.y, currentModel.transform.rotation.z, currentModel.transform.rotation.w);

        DeleteModel();

        var currModel = Instantiate(model, currPosition, currRotation);
        Debug.Log($"** Model with name {model.name} created.");
    }

    /// <summary>
    /// Create cutting plane and map it to the tracked HHD
    /// Maximum of 3 cutting planes allowed
    /// Add cutting plane to the current model
    /// </summary>
    public void CreateCuttingPlane()
    {
        Debug.Log("Create cutting plane");

        var trans = GetTrackingCubeTransform();
        if (!trans)
        {
            Debug.LogError("Tracking cube could not be found.");
            return;
        }

        var currModel = FindCurrentModel();

        if (!currModel)
        {
            Debug.Log("No model found to attach cutting plane to.");
            return;
        }

        var cuttingScript = currModel.GetComponent<GenericThreePlanesCuttingController>();
        if (!cuttingScript.plane3.name.Contains(StringConstants.Empty))
        {
            Debug.Log("Maximum of three cutting planes reached.");
            return;
        }

        var newCuttingPlane = Instantiate(sectionQuad, new Vector3(trans.position.x - 0.1f, trans.position.y, trans.position.z), Quaternion.identity, tracker.transform);
        SetModelCuttingPlane(newCuttingPlane, cuttingScript);
    }

    private void SetModelCuttingPlane(GameObject plane, GenericThreePlanesCuttingController cuttingScript)
    {        
        if (cuttingScript.plane1.name.Contains(StringConstants.Empty))
        {
            cuttingScript.plane1 = plane;
        }
        else if (cuttingScript.plane2.name.Contains(StringConstants.Empty))
        {
            cuttingScript.plane2 = plane;
        }
        else if (cuttingScript.plane3.name.Contains(StringConstants.Empty))
        {
            cuttingScript.plane3 = plane;
        }
    }

    /// <summary>
    /// Delete all existing cutting planes
    /// </summary>
    public void DeleteAllCuttingPlanes()
    {
        Debug.Log("Delete cutting planes");

        var goToBeDestroyed = new List<GameObject>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.name.Contains($"{sectionQuad.name}{StringConstants.Clone}"))
            {
                goToBeDestroyed.Add(go);
            }
        }

        var currModel = FindCurrentModel();
        if (currModel)
        {
            var empty = currModel.transform.Find(StringConstants.Empty).gameObject;
            var currModelScript = currModel.GetComponent<GenericThreePlanesCuttingController>();
            currModelScript.plane1 = empty;
            currModelScript.plane2 = empty;
            currModelScript.plane3 = empty;
        }

        goToBeDestroyed.ForEach(go => Destroy(go));
    }

    /// <summary>
    /// Stop mapping between cutting plane and tracked HHD
    /// </summary>
    public void DispatchCurrentCuttingPlane()
    {
        Debug.Log("Dispatch current cutting plane");
        var cuttingPlane = tracker.transform.Find($"{sectionQuad.name}{StringConstants.Clone}");

        if (cuttingPlane == null)
        {
            Debug.Log($"** No cutting plane with name {sectionQuad.name} found.");
        }
        else
        {
            cuttingPlane.transform.SetParent(null);
        }

        //check if dummy cuttingplane needs to be set
    }

    public void DeleteAllSnapshots()
    {
        Debug.Log("Delete snapshots");

        var goToBeDestroyed = new List<GameObject>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.name.Contains(StringConstants.Snapshot))
            {
                goToBeDestroyed.Add(go);
            }
        }

        goToBeDestroyed.ForEach(go => Destroy(go));
    }

    /// <summary>
    /// Align objects around handheld device
    /// Avoid Overlapping
    /// There should not be too many objects due to limited size
    /// https://answers.unity.com/questions/1068513/place-8-objects-around-a-target-gameobject.html
    /// Set tracker as parent so future movements are mapped
    /// </summary>
    public void AlignSnapshots()
    {
        Debug.Log("Align snapshots");

        var snapshots = new List<GameObject>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.name.Contains(StringConstants.Snapshot + StringConstants.Clone))
            {
                snapshots.Add(go);
            }
        }

        var overlay = tracker.transform.FindChild(StringConstants.OverlayScreen);
        if (!overlay)
        {
            Debug.Log("Alignment not possible. Overlay screen not found as child of tracker.");
        }

        if (snapshots.Count > 5)
        {
            Debug.Log("More than 5 snapshots detected. Only 5 will be aligned.");
        }

        var index = 1;
        foreach (var shot in snapshots)
        {
            var child = overlay.GetChild(index++);
            child.gameObject.SetActive(true);
            shot.transform.SetParent(child);
            shot.GetComponent<Viewable>().IsLookingAt = false;
            shot.transform.position = child.position;
            shot.transform.rotation = new Quaternion();
        }

        // align in a circle
        //var radius = snapshots.Count * 2;
        //for (int i = 0; i < snapshots.Count; i++)
        //{
        //    var angle = i * Mathf.PI * 2f / radius;
        //    var newPos = tracker.transform.position + new Vector3(Mathf.Cos(angle) * radius, -2, Mathf.Sin(angle) * radius);
        //    snapshots[i].transform.position = newPos;
        //    snapshots[i].transform.SetParent(tracker.transform);
        //}
    }

    private Transform GetTrackingCubeTransform()
    {
        return tracker.transform.GetChild(0);
    }
}
