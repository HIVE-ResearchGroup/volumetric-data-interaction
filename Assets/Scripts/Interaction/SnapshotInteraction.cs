using Assets.Scripts.Exploration;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SnapshotInteraction : MonoBehaviour
{
    public GameObject Tracker;

    private float snapshotTimer = 0.0f;
    private float snapshotThreshold = 3.0f;

    void Update()
    {
        if (snapshotTimer <= snapshotThreshold)
        {
            snapshotTimer += Time.deltaTime;
        }
    }

    public void HandleSnapshotCreation(float angle)
    {
        if (angle > 0 || snapshotThreshold > snapshotTimer) // means downward swipe - no placement
        {
            return;
        }

        snapshotTimer = 0f;
        var currPos = Tracker.transform.position;
        var currRot = Tracker.transform.rotation;
        var centeringRotation = -90;

        var newPosition = currPos + Quaternion.AngleAxis(angle + currRot.eulerAngles.y + centeringRotation, Vector3.up) * Vector3.back * ConfigurationConstants.SNAPSHOT_DISTANCE;
        PlaceSnapshot(newPosition);
    }

    public bool HasSnapshots()
    {
        return GetAllSnapshots().Count != 0;
    }

    public bool IsSnapshot(GameObject selectedObject)
    {
        if (selectedObject == null)
        {
            return false;
        }
        return (selectedObject?.name.Contains(StringConstants.Snapshot) ?? false)  || IsSnapshotNeighbour(selectedObject);
    }

    private bool IsSnapshotNeighbour(GameObject selectedObject)
    {
        return selectedObject?.name.Contains(StringConstants.Neighbour) ?? false;
    }    

    private List<GameObject> GetAllSnapshots()
    {
        var snapshots = new List<GameObject>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (IsSnapshot(go))
            {
                snapshots.Add(go);
            }
        }

        return snapshots;
    }

    public bool DeleteSnapshotsIfExist(GameObject selectedObject, int shakeCounter)
    {
        if (selectedObject && IsSnapshot(selectedObject)) {
            DeleteSnapshot(selectedObject);
            return true;
        }
        else if (shakeCounter > 1 && !selectedObject && GetAllSnapshots().Count > 1)
        {
            DeleteAllSnapshots();
            return true;
        }
        return false;
    }

    public void DeleteAllSnapshots()
    {
        var snapshots = GetAllSnapshots();
        snapshots.ForEach(DeleteSnapshot);
    }

    public void DeleteSnapshot(GameObject snapshot)
    {
        var snapshotScript = snapshot.GetComponent<Snapshot>();
        snapshotScript.SetSelected(false);
        Destroy(snapshotScript.OriginPlane);
        Destroy(snapshot);
    }

    /// <summary>
    /// It could happen that nor all snapshots are aligned due to the size restriction
    /// </summary>
    private bool AreSnapshotsAligned(List<GameObject> snapshots)
    {
        foreach (var snap in snapshots)
        {
            if (!snap.GetComponent<Snapshot>().IsLookingAt)
            {
                return true;
            }
        }
        return false;
    }

    public void AlignOrMisAlignSnapshots()
    {
        if (snapshotTimer <= snapshotThreshold)
        {
            return;
        }

        snapshotTimer = 0f;
        var snapshots = GetAllSnapshots();
        var areSnapshotsAligned = AreSnapshotsAligned(snapshots);

        if (areSnapshotsAligned)
        {
            MisalignSnapshots(snapshots);
        }
        else
        {
            AlignSnapshots(snapshots);
        }
    }

    /// <summary>
    /// Only up to 5 snapshots can be aligned. The rest needs to stay in their original position
    /// </summary>
    private void AlignSnapshots(List<GameObject> snapshots)
    {
        var overlay = Tracker.transform.FindChild(StringConstants.OverlayScreen);
        if (!overlay)
        {
            Debug.Log("Alignment not possible. Overlay screen not found as child of tracker.");
        }

        for (int index = 0; index < snapshots.Count && index < 5; index++)
        {
            var shot = snapshots[index];
            var child = overlay.GetChild(index + 1); // first child is main overlay
            shot.GetComponent<Snapshot>().SetAligned(overlay);
            shot.transform.position = child.position;
            shot.transform.rotation = new Quaternion();
            shot.transform.localScale = child.localScale;
        }
    }

    public void MisalignSnapshots(List<GameObject> snapshots)
    {
        foreach (var shot in snapshots)
        {
            shot.GetComponent<Snapshot>().SetMisaligned();
        }
    }

    public void PlaceSnapshot(Vector3 newPosition)
    {
        var snapshotPrefab = Resources.Load(StringConstants.PrefabSnapshot, typeof(GameObject)) as GameObject;
        var snapshot = Instantiate(snapshotPrefab);
        snapshot.transform.position = newPosition;

        GameObject modelGo;
        try
        {
            modelGo = GameObject.Find(StringConstants.ModelName) ?? GameObject.Find($"{StringConstants.ModelName}({StringConstants.Clone})");
            var model = modelGo.GetComponent<Model>();
            var (snapshotTexture, snapshotPlane) = model.GetIntersectionAndTexture();
            SetTexture(snapshot, snapshotTexture);
            snapshot.GetComponent<Snapshot>().SetPlaneCoordinates(snapshotPlane);
        }
        catch (Exception e)
        {
            Destroy(snapshot);
            return;
        }

        // set origin plane
        var main = GameObject.Find(StringConstants.Main);
        var originPlane = Instantiate(Resources.Load(StringConstants.PrefabOriginPlane), main.transform.position, main.transform.rotation) as GameObject;
        originPlane.transform.SetParent(modelGo.transform);

        var snapshotScript = snapshot.GetComponent<Snapshot>();
        snapshotScript.Viewer = Tracker;
        snapshotScript.OriginPlane = originPlane;
        snapshotScript.SetSelected(false);
    }

    public void GetNeighbour(bool isLeft, GameObject selectedObject)
    {
        var overlay = Tracker.transform.FindChild(StringConstants.OverlayScreen);
        if (!IsSnapshot(selectedObject) || overlay == null)
        {
            return;
        }
       
        var selectedSnapshot = selectedObject.GetComponent<Snapshot>();
        var originalPlaneCoordinates = selectedSnapshot.GetPlaneCoordinates();

        var neighbourGo = CreateNeighbourGameobject();
        try
        {
            var model = (GameObject.Find(StringConstants.ModelName) ?? GameObject.Find($"{StringConstants.ModelName}({StringConstants.Clone})")).GetComponent<Model>();
            var slicePlane = new SlicePlane(model, originalPlaneCoordinates);
            var (texture, startPoint) = slicePlane.CalculateNeighbourIntersectionPlane(isLeft);
                        
            var neighbourSnap = neighbourGo.GetComponent<Snapshot>();
            neighbourSnap.InstantiateForGo(selectedSnapshot);
            neighbourSnap.SetSnapshotTexture(texture);

            if (IsNeighbourStartPointDifferent(originalPlaneCoordinates.StartPoint, startPoint))
            {
                var neighbourPlane = new SlicePlaneCoordinates(originalPlaneCoordinates, startPoint);
                neighbourSnap.SetPlaneCoordinates(neighbourPlane);
            }
            else
            {
                Debug.Log("No more neighbour in this direction");
            }

            var host = GameObject.Find(StringConstants.Host).GetComponent<Host>();
            host.ChangeSelectedObject(neighbourGo);

            SetTexture(neighbourGo, texture);
            neighbourSnap.SetOverlayTexture(true);
            neighbourGo.SetActive(false);
        }
        catch (Exception e)
        {
            Destroy(neighbourGo);
            return;
        }
    }

    private void SetTexture(GameObject gameObject, Texture2D texture)
    {
        var renderer = gameObject.GetComponent<MeshRenderer>();
        renderer.material.mainTexture = texture;
        renderer.material.mainTextureScale = new Vector2(-1, -1);
    }

    private GameObject CreateNeighbourGameobject()
    {
        var snapshotPrefab = Resources.Load(StringConstants.PrefabSnapshot, typeof(GameObject)) as GameObject;
        var neighbourGo = Instantiate(snapshotPrefab);
        neighbourGo.name = StringConstants.Neighbour;
        Destroy(neighbourGo.GetComponent<Selectable>());
        return neighbourGo;
    }

    private bool IsNeighbourStartPointDifferent(Vector3 originalStartpoint, Vector3 neighbourStartpoint)
    {
        return originalStartpoint != neighbourStartpoint;
    } 

    public void CleanUpNeighbours()
    {
        var snapshots = new List<GameObject>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (IsSnapshotNeighbour(go))
            {
                snapshots.Add(go);
            }
        }

        snapshots.ForEach(s => Destroy(s));
    }

    public void DeactivateAllSnapshots()
    {
        var snapshots = GetAllSnapshots();
        foreach (var snap in snapshots)
        {
            snap.GetComponent<Snapshot>()?.SetSelected(false);
        }
    }
}