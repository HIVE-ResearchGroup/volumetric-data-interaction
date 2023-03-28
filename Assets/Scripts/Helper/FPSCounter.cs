using UnityEngine;

namespace Assets.Scripts.Helper
{
    public class FPSCounter : MonoBehaviour
    {
        public int FPS;

        void Update()
        {
            FPS = (int)(1f / Time.deltaTime);
        }
    }
}
