using System.Collections.Generic;
using UnityEngine;

namespace Interaction
{
    public class TabletOverlay : MonoBehaviour
    {
        public Transform Main { get; private set; }

        public List<Transform> Additions { get; } = new();

        private void Awake()
        {
            // the first one is main
            // get all additions and add them to the list
            Main = transform.GetChild(0);
            for (var i = 0; i < transform.childCount - 1; i++)
            {
                Additions[i] = transform.GetChild(i + 1);
            }
        }
    }
}