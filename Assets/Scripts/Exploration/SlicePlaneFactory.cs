using System.Collections.Generic;
using UnityEngine;

namespace Exploration
{
    public class SlicePlaneFactory : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip cameraSound;

        [SerializeField]
        private Texture2D invalidTexture;

        public SlicePlane Create(Model model, SlicePlaneCoordinates plane)
        {
            return new SlicePlane(model, audioSource, cameraSound, invalidTexture, plane);
        }

        public SlicePlane Create(Model model, IReadOnlyList<Vector3> intersectionPoints)
        {
            return new SlicePlane(model, audioSource, cameraSound, invalidTexture, intersectionPoints);
        }
    }
}
