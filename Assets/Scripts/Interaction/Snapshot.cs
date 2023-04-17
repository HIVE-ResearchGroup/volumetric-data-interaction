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
    private MeshRenderer mainRenderer;
    private Texture mainOverlayTexture;
    private Vector3 misalignedPosition;
    public Vector3 misalignedScale;

    private GameObject tempNeighbourOverlay;

    public SlicePlaneCoordinates PlaneCoordinates { get; set; }
    public Texture SnapshotTexture { get; set; }

    public void InstantiateForGo(Snapshot otherSnapshot, Vector3 originPlanePosition)
    {
        Viewer = otherSnapshot.Viewer;
        IsLookingAt = false;
        model = otherSnapshot.model;
        mainOverlay = otherSnapshot.mainOverlay;
        mainOverlayTexture = otherSnapshot.mainOverlayTexture;
        SnapshotTexture = otherSnapshot.SnapshotTexture;
        misalignedPosition = otherSnapshot.misalignedPosition;
        misalignedScale = otherSnapshot.misalignedScale;
        OriginPlane = otherSnapshot.OriginPlane;
        OriginPlane.transform.position = originPlanePosition;
    }

    private GameObject GetTextureQuad() => transform.GetChild(0).gameObject;

    private void Start()
    {
        mainOverlay = GameObject.Find(StringConstants.Main);
        mainRenderer = mainOverlay.GetComponent<MeshRenderer>();
        mainOverlayTexture = mainRenderer.material.mainTexture;
        model = ModelFinder.FindModelGameObject().GetComponent<Model>();

        SnapshotTexture = GetTextureQuad().GetComponent<MeshRenderer>().material.mainTexture;
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

        if (isSelected)
        { 
            var black = Resources.Load(StringConstants.MaterialBlack, typeof(Material)) as Material;
            mainRenderer.material = black;

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
            mainRenderer.material = main;
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