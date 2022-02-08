using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interactions class has to be attached to gameobject holding the tracked device
/// </summary>
public class Interactions : MonoBehaviour
{
    public GameObject sectionQuad;
    public GameObject model;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) 
        {
            CreateModel();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            DeleteModel();
        }
        else if (Input.GetKeyDown(KeyCode.Z)) 
        {
            ResetModel();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            CreateCuttingPlane();
        }
        else if (Input.GetKeyDown(KeyCode.I)) 
        {
            DeleteCuttingPlanes();
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            DispatchCurrentCuttingPlane();
        }
    }

    /// <summary>
    /// Create model at specified position
    /// e.g. 5 cm before HHD
    /// Do not allow multiple models!
    /// Set dummy cuttingPlane for cuttingScript to avoid error
    /// </summary>
    public void CreateModel()
    {
        Debug.Log("Create model");

        var currModel = FindCurrentModel();

        if (currModel)
        {
            DeleteModel();
        }

        var currTrackingPosition = gameObject.transform.position;
        var currTrackingRotation = gameObject.transform.rotation; //todo - adjust if necesssary

        currModel = Instantiate(model, new Vector3(currTrackingPosition.x, currTrackingPosition.y, currTrackingPosition.z + 5), Quaternion.identity);
        SetDummyCuttingPlane(currModel);
    }

    private GameObject FindCurrentModel()
    {
        var currModel = GameObject.Find(model.name) ?? GameObject.Find($"{model.name}(Clone)");
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
            DeleteCuttingPlanes();
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
        SetDummyCuttingPlane(currModel);
        Debug.Log($"** Model with name {model.name} created.");
    }

    /// <summary>
    /// Create cutting plane and map it to the tracked HHD
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
        var newCuttingPlane = Instantiate(sectionQuad, new Vector3(trans.position.x - 0.1f, trans.position.y, trans.position.z), Quaternion.identity);
        //newCuttingPlane.transform.rotation = new Quaternion(trans.rotation.z, trans.rotation.x, trans.rotation.y, 0);
        newCuttingPlane.transform.SetParent(gameObject.transform);

        var currModel = FindCurrentModel();
        if (currModel)
        {
            var cuttingScript = currModel.GetComponent<OnePlaneCuttingController>();
            cuttingScript.plane = newCuttingPlane;
        }
    }

    private void SetDummyCuttingPlane(GameObject currentModel)
    {
        var cuttingScript = currentModel.GetComponent<OnePlaneCuttingController>();
        var dummyCuttingPlane = Instantiate(sectionQuad, new Vector3(0, 0, 0), Quaternion.identity);
        dummyCuttingPlane.transform.SetParent(null);
        dummyCuttingPlane.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        cuttingScript.plane = dummyCuttingPlane;
    }

    /// <summary>
    /// Delete all existing cutting planes
    /// </summary>
    public void DeleteCuttingPlanes()
    {
        Debug.Log("Delete cutting planes");

        var goToBeDestroyed = new List<GameObject>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (go.name.Contains($"{sectionQuad.name}(Clone)"))
            {
                goToBeDestroyed.Add(go);
            }
        }

        var currModel = FindCurrentModel();
        if (currModel)
        {
            SetDummyCuttingPlane(currModel);
        }

        goToBeDestroyed.ForEach(go => DestroyImmediate(go, true));
    }

    /// <summary>
    /// Stop mapping between cutting plane and tracked HHD
    /// </summary>
    public void DispatchCurrentCuttingPlane()
    {
        Debug.Log("Dispatch current cutting plane");
        var cuttingPlane = transform.Find($"{sectionQuad.name}(Clone)");

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

    private Transform GetTrackingCubeTransform()
    {
        return gameObject.transform.GetChild(0);
    }
}
