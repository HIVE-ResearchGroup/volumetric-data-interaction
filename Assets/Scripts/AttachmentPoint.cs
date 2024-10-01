#nullable enable

using Snapshots;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class AttachmentPoint : MonoBehaviour
{
    public bool HasAttachment => snapshot != null;

    private MeshRenderer meshRenderer = null!;

    private Snapshot? snapshot;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Attach(Snapshot s)
    {
        if (snapshot != null)
        {
            Debug.LogError("Object is already attached to this point!");
            return;
        }
        snapshot = s;
        meshRenderer.material.mainTexture = s.SnapshotTexture;
        s.gameObject.SetActive(false);
        meshRenderer.enabled = true;
    }

    public void Detach()
    {
        if (snapshot == null)
        {
            return;
        }

        snapshot.gameObject.SetActive(true);
        meshRenderer.enabled = false;
    }
}
