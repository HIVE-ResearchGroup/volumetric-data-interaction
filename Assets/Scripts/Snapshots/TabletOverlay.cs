using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Snapshots
{
    public class TabletOverlay : MonoBehaviour
    {
        public const int AdditionCount = 5;

        [SerializeField]
        private Transform main;

        private MeshRenderer _mainMeshRenderer;

        public Transform Main => main;

        public List<Transform> Additions { get; } = new(AdditionCount);

        private void Awake()
        {
            _mainMeshRenderer = Main.GetComponent<MeshRenderer>();
            
            // the first one is main
            // get all additions and add them to the list
            for (var i = 0; i < AdditionCount; i++)
            {
                Additions.Add(transform.GetChild(i + 1));
            }
        }

        public void SetMaterial([NotNull] Material mat) => _mainMeshRenderer.material = mat;
    }
}