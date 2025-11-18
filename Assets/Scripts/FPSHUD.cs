using UnityEngine;

public class FPSHUD : MonoBehaviour
{
    [SerializeField] private float refreshRate = 0.5f;

    private TMPro.TMP_Text fpsText;
    private float timer = 0f;

    private void Start()
    {
        fpsText = GetComponent<TMPro.TMP_Text>();
    }

    private void Update()
    {
        if (Time.unscaledTime > timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            fpsText.text = "FPS: " + fps;
            timer = Time.unscaledTime + refreshRate;
        }
    }
}
