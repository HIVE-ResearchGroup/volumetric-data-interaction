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

    private int width;
    private int height;
    private Vector3 calculationStartPoint;
    private Vector3 calculationWidthSteps;
    private Vector3 calculationHeightSteps;

    private void Start()
    {
        mainOverlay = GameObject.Find(StringConstants.Main);
        model = GameObject.Find(StringConstants.ModelName).GetComponent<Model>();
        mainOverlayTexture = mainOverlay.GetComponent<MeshRenderer>().material.mainTexture;
        snapshotTexture = gameObject.GetComponent<MeshRenderer>().material.mainTexture;
    }

    private void Update()
    {
        if (IsLookingAt)
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

    public void GetNeightbour(bool isLeft)
    {
        var neighbour = model.CalculateIntersectionPlane(width, height, calculationStartPoint, calculationWidthSteps, calculationHeightSteps);
        // do something
    }
}