using UnityEngine;
using TMPro;
using System;
using UnityEngine.Video;
using System.Collections;

public class IntroCutscene : MonoBehaviour
{
    [SerializeField] private GameObject GameUI;
    [SerializeField] private GameObject IntroCutsceneUI;
    [SerializeField] private GameObject skipText;

    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseScaleMin = 0.95f;
    [SerializeField] private float pulseScaleMax = 1.05f;
    [SerializeField] private float fadeSpeed = 1f;

    [SerializeField] private VideoPlayer unskippableVideoPlayer;
    [SerializeField] private VideoPlayer skippableVideoPlayer;
    [SerializeField] private GameObject unskippableScreen;
    [SerializeField] private GameObject skippableScreen;

    private Vector3 originalScale;

    private bool isActive = true;
    private bool isSkippable = false;

    IEnumerator Start()
    {
        if (isActive)
        {
            GameUI.SetActive(false);
            skippableScreen.SetActive(false);
            skippableVideoPlayer.gameObject.SetActive(false);
            originalScale = skipText.transform.localScale;

            float globalScale = 0.95f;
            unskippableScreen.transform.localScale = Vector3.one * globalScale;

            yield return new WaitForSeconds(0.1f);

            skipText.gameObject.SetActive(false);
            isSkippable = false;

            unskippableScreen.SetActive(true);
            unskippableVideoPlayer.gameObject.SetActive(true);

            unskippableVideoPlayer.loopPointReached += OnUnskippableClipFinished;
            unskippableVideoPlayer.Play();
        }
    }

    void Update()
    {
        if (!isActive)
            return;

        if (isSkippable)
        {
            skipText.gameObject.SetActive(true);
            UpdateSkipText();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndCutscene();
            }
        }
        else
        {
            skipText.gameObject.SetActive(false);
        }
    }

    private void OnUnskippableClipFinished(VideoPlayer vp)
    {
        unskippableVideoPlayer.loopPointReached -= OnUnskippableClipFinished;
        unskippableScreen.SetActive(false);
        unskippableVideoPlayer.gameObject.SetActive(false);

        skippableScreen.SetActive(true);
        skippableVideoPlayer.gameObject.SetActive(true);

        skippableVideoPlayer.loopPointReached += OnSkippableClipFinished;
        skippableVideoPlayer.Play();

        isSkippable = true;
    }

    private void OnSkippableClipFinished(VideoPlayer vp)
    {
        skippableVideoPlayer.loopPointReached -= OnSkippableClipFinished;
        EndCutscene();
    }

    private void EndCutscene()
    {
        isActive = false;
        GameUI.SetActive(true);
        IntroCutsceneUI.SetActive(false);

        unskippableVideoPlayer.Stop();
        skippableVideoPlayer.Stop();

        unskippableScreen.SetActive(false);
        skippableScreen.SetActive(false);
    }

    private void UpdateSkipText()
    {
        float scale = Mathf.Lerp(pulseScaleMin, pulseScaleMax, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        skipText.transform.localScale = originalScale * scale;
    }
}
