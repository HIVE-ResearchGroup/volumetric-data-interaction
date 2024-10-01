using UnityEngine;

namespace Extensions
{
    public static class SnapshotExtensions
    {
        public static bool IsSnapshot(this GameObject obj) => obj.CompareTag(Tags.Snapshot);
    }
}