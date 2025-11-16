using TMPro;
using UnityEngine;
using System.Collections;

public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float seconds = 2f;

    void Awake() => Instance = this;

    public void Show(string msg, float? duration = null)
    {
        if (!label) return;
        StopAllCoroutines();
        label.text = msg;
        label.gameObject.SetActive(true);
        StartCoroutine(HideAfter(duration ?? seconds));
    }

    IEnumerator HideAfter(float t)
    {
        yield return new WaitForSeconds(t);
        label.gameObject.SetActive(false);
    }
}
