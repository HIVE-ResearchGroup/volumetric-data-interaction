using UnityEngine;

namespace Networking.Screens
{
    [RequireComponent(typeof(BoxCollider))]
    public class Screen : MonoBehaviour
    {
        [SerializeField]
        private int id;

        public int ID => id;

        public BoxCollider BoxCollider { get; private set; }

        private void Awake()
        {
            BoxCollider = GetComponent<BoxCollider>();
        }
    }
}