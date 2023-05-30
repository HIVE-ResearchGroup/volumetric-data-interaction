using Interaction;
using UnityEngine;

namespace Networking
{
    public class HostReferencesManager : MonoBehaviour
    {
        [SerializeField]
        public GameObject main;
        [SerializeField]
        public MeshRenderer mainRenderer;
        [SerializeField]
        public Interaction.Exploration analysis;
        [SerializeField]
        public InterfaceVisualisation ui;
        [SerializeField]
        public SpatialInteraction spatialHandler;
        [SerializeField]
        public SnapshotInteraction snapshotHandler;

        [SerializeField]
        public GameObject ray;
    }
}
