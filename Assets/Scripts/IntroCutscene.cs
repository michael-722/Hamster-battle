using UnityEngine;
using TMPro;
using System;

public class IntroCutscene : MonoBehaviour
{
    [SerializeField] private GameObject GameUI;
    [SerializeField] private GameObject IntroCutsceneUI;

    [SerializeField] private TMP_Text skipText;


    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseScaleMin = 0.95f;
    [SerializeField] private float pulseScaleMax = 1.05f;
    [SerializeField] private float fadeSpeed = 1f;

    private float currentAlpha = 1f;
    private Vector3 originalScale;


    private bool isActive = true;

    void Start()
    {
        if (isActive)
        {
            GameUI.SetActive(false);
            IntroCutsceneUI.SetActive(true);
            originalScale = skipText.transform.localScale;
            currentAlpha = 1f;
        }
    }

    void Update()
    {
        if (!isActive)
            return;

        UpdateSkipText();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isActive = false;
            GameUI.SetActive(true);
            IntroCutsceneUI.SetActive(false);
        }
    }

    private void UpdateSkipText()
    {
        float scale = Mathf.Lerp(pulseScaleMin, pulseScaleMax, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        skipText.transform.localScale = originalScale * scale;
        float minAlpha = 0.7f;
        float maxAlpha = 0.9f;

        currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

        Color c = skipText.color;
        c.a = currentAlpha;
        skipText.color = c;
    }
}
