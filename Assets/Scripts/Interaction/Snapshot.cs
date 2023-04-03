using Assets.Scripts.Exploration;
using Assets.Scripts.Helper;
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

    private GameObject tempNeighbourOverlay;

    private SlicePlaneCoordinates planeCoordinates;

    public Snapshot(Snapshot otherSnapshot)
    {
        Viewer = otherSnapshot.Viewer;
        IsLookingAt = otherSnapshot.IsLookingAt;
        OriginPlane = otherSnapshot.OriginPlane;
    }

    public void InstantiateForGo(Snapshot otherSnapshot, Vector3 originPlanePosition)
    {
        Viewer = otherSnapshot.Viewer;
        IsLookingAt = false;
        model = otherSnapshot.model;
        mainOverlay = otherSnapshot.mainOverlay;
        mainOverlayTexture = otherSnapshot.mainOverlayTexture;
        snapshotTexture = otherSnapshot.snapshotTexture;
        misalignedPosition = otherSnapshot.misalignedPosition;
        misalignedScale = otherSnapshot.misalignedScale;
        OriginPlane = otherSnapshot.OriginPlane;
        OriginPlane.transform.position = originPlanePosition;
    }

    private GameObject GetTextureQuad()
    {
        return transform.GetChild(0).gameObject;
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
        model = ModelFinder.FindModelGameObject().GetComponent<Model>();
        mainOverlayTexture = mainOverlay.GetComponent<MeshRenderer>().material.mainTexture;

        snapshotTexture = GetTextureQuad().GetComponent<MeshRenderer>().material.mainTexture;
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
            var black = Resources.Load(StringConstants.MaterialBlack, typeof(Material)) as Material;
            renderer.material = black;

            var overlay = mainOverlay.transform;
            var snapshotQuad = Instantiate(GetTextureQuad());
            var scale = MaterialAdjuster.GetAspectRatioSize(overlay.localScale, snapshotQuad.transform.localScale.y, snapshotQuad.transform.localScale.x); //new Vector3(1, 0.65f, 0.1f);
            
            snapshotQuad.transform.SetParent(mainOverlay.transform);
            snapshotQuad.transform.localScale = scale;
            snapshotQuad.transform.localPosition = new Vector3(0, 0, -0.1f);
            snapshotQuad.transform.localRotation = new Quaternion();
            Destroy(tempNeighbourOverlay);
            tempNeighbourOverlay = snapshotQuad;
        }
        else
        {
            var main = Resources.Load(StringConstants.MaterialUIMain, typeof(Material)) as Material;
            renderer.material = main;
            Destroy(tempNeighbourOverlay);
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