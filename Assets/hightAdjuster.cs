using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class hightAdjuster : MonoBehaviour
{
    private int previousChildCount = 0;
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = this.gameObject.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        rectTransform = this.gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, (transform.childCount * 70) + 50);
    }

    private void FixedUpdate()
    {
        rectTransform.sizeDelta = new Vector2(0, (transform.childCount * 70) + 50);
    }
}
