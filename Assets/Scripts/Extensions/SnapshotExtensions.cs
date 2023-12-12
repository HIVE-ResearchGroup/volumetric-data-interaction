using Constants;
using UnityEngine;

namespace Extensions
{
    public static class SnapshotExtensions
    {
        public static bool IsSnapshot(this GameObject obj) => obj.CompareTag(Tags.Snapshot) || obj.IsNeighbour();

        public static bool IsNeighbour(this GameObject obj) => obj.CompareTag(Tags.SnapshotNeighbour);
    }
}