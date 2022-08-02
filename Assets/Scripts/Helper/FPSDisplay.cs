using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    public float FPS;

    void Update()
    {
        float current = (int)(1f / Time.deltaTime);
        if (Time.frameCount % 50 == 0)
        {
            FPS = current;
        }
    }
}
