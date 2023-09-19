using UnityEngine;

namespace Interaction
{
    public class TabletOverlay : MonoBehaviour
    {
        [SerializeField]
        private GameObject main;

        public GameObject Main => main;
    }
}