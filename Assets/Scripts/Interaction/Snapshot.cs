using Assets.Scripts.Exploration;
using UnityEngine;

public class Snapshot : MonoBehaviour
{
    public GameObject Viewer;
    public bool IsLookingAt = true;
    public GameObject OriginPlane;

    private Model model;
    private GameObject mainOverlay;
    private Texture mainOverlayTexture;
    private Texture snapshotTexture;
    private Vector3 misalignedPosition;
    public Vector3 misalignedScale;

    private SlicePlaneCoordinates planeCoordinates;

    public Snapshot(Snapshot otherSnapshot)
    {
        Viewer = otherSnapshot.Viewer;
        IsLookingAt = otherSnapshot.IsLookingAt;
        OriginPlane = otherSnapshot.OriginPlane;
    }

    private void Start()
    {
        mainOverlay = GameObject.Find(StringConstants.Main);
        model = GameObject.Find(StringConstants.ModelName).GetComponent<Model>();
        mainOverlayTexture = mainOverlay.GetComponent<MeshRenderer>().material.mainTexture;
        snapshotTexture = gameObject.GetComponent<MeshRenderer>().material.mainTexture;
    }

    private void Update()
    {
        if (Viewer && IsLookingAt)
        {
            gameObject.transform.LookAt(Viewer.transform);
            transform.forward = -transform.forward; //need to adjust as quad is else not visible
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SetSelected(true, true);
    }

    private void OnTriggerExit(Collider other)
    {
        SetSelected(false, true);
    }

    public void SetSelected(bool isSelected, bool isTrigger = false)
    {
        if (!OriginPlane)
        {
            return;
        }
        
        OriginPlane.SetActive(isSelected);

        if (mainOverlay)
        {
            mainOverlay.GetComponent<MeshRenderer>().material.mainTexture = isSelected ? snapshotTexture : mainOverlayTexture;
        }
    }

    public void SetAligned(Transform overlay)
    {
        misalignedScale = transform.localScale;
        misalignedPosition = transform.localPosition;
        IsLookingAt = false;
        transform.SetParent(overlay);
    }

    public void SetMisaligned()
    {
        transform.SetParent(null);
        transform.localScale = misalignedScale; 
        transform.position = misalignedPosition;
        IsLookingAt = true;
    }

    public Snapshot GetNeightbourSlice(bool isForward)
    {
        var currPlane = new SlicePlane(model, planeCoordinates);
        var (slice, startPoint) = currPlane.CalculateNeighbourIntersectionPlane(isForward);
        var neighbour = new Snapshot(this);
        neighbour.model = this.model;
        neighbour.mainOverlay = this.mainOverlay;
        neighbour.planeCoordinates = this.planeCoordinates;
        neighbour.planeCoordinates.StartPoint = startPoint;
        neighbour.mainOverlayTexture = this.mainOverlayTexture;
        // TODO - set slicetexture
        return neighbour;
    }
}