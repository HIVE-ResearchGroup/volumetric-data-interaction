#nullable enable

using UnityEngine;

public class MappingAnchor : MonoBehaviour
{
    [SerializeField]
    private Transform tracker = null!;

    private Transform? model;
    
    private Transform? parent;
    
    private Vector3 positionOffset;
    
    private void Update()
    {
        // tracker is null if not mapping
        if (model == null)
        {
            return;
        }

        var mappingTransform = transform;
        mappingTransform.SetPositionAndRotation(tracker.position + positionOffset, tracker.rotation);

        var mappingPosition = mappingTransform.position;
        Debug.DrawRay(mappingPosition, mappingTransform.up, Color.green);
        Debug.DrawRay(mappingPosition, mappingTransform.forward, Color.blue);
        Debug.DrawRay(mappingPosition, mappingTransform.right, Color.red);
    }
    
    public void StartMapping(Transform newModel)
    {
        if (model != null)
        {
            Debug.Log("Mapping was not stopped before. Stopping now.");
            StopMapping();
        }

        Debug.Log("Started Mapping");
        model = newModel;
        parent = newModel.parent;
        
        var modelPosition = newModel.position;
        positionOffset = modelPosition - tracker.position;

        var mappingTransform = transform;
        mappingTransform.SetPositionAndRotation(modelPosition, tracker.rotation);
        newModel.SetParent(mappingTransform);
    }

    public bool StopMapping()
    {
        if (model == null)
        {
            return false;
        }
        
        Debug.Log("Stopped Mapping");
        model.SetParent(parent);
        model = null;
        parent = null;
        return true;
    }
}
