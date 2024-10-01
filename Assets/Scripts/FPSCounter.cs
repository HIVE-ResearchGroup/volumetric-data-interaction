using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    void Update()
    {
        text.SetText($"FPS: {(int)(1f / Time.deltaTime)}");
    }
}
