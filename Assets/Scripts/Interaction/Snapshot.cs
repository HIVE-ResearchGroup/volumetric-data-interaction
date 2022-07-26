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

    public void InstantiateForGo(Snapshot otherSnapshot)
    {
        Viewer = otherSnapshot.Viewer;
        IsLookingAt = false;
        model = otherSnapshot.model;
        mainOverlay = otherSnapshot.mainOverlay;
        mainOverlayTexture = otherSnapshot.mainOverlayTexture;
        snapshotTexture = otherSnapshot.snapshotTexture;
        misalignedPosition = otherSnapshot.misalignedPosition;
        misalignedScale = otherSnapshot.misalignedScale;
    }

    public void SetPlaneCoordinates(SlicePlaneCoordinates plane)
    {
        planeCoordinates = plane;
    }

    public void SetSnapshotTexture(Texture texture)
    {
        snapshotTexture = texture;
    }

    public SlicePlaneCoordinates GetPlaneCoordinates()
    {
        return planeCoordinates;
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
        if (other.name == StringConstants.Ray)
        {
            SetSelected(true, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == StringConstants.Ray)
        {
            SetSelected(false, true);
        }
    }

    public void SetSelected(bool isSelected, bool isTrigger = false)
    {
        if (!OriginPlane)
        {
            return;
        }
        
        OriginPlane.SetActive(isSelected);

        SetOverlayTexture(isSelected);
    }

    public void SetOverlayTexture(bool isSelected)
    {
        if (!mainOverlay)
        {
            return;
        }

        var renderer = mainOverlay.GetComponent<MeshRenderer>();
        if (isSelected)
        { 
            var white = Resources.Load(StringConstants.MaterialWhite, typeof(Material)) as Material;
            renderer.material = white;
            renderer.material.mainTexture = snapshotTexture;
            renderer.material.mainTextureScale = isSelected ? new Vector2(-1, -1) : new Vector2(1,1);
        }
        else
        {
            var main = Resources.Load(StringConstants.MaterialUIMain, typeof(Material)) as Material;
            renderer.material = main;
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
}