using UnityEngine;

public class CreditsScroll : MonoBehaviour
{
    public bool startScroll;
    Vector2 startPos;
    public float scrollSpeed = 0.1f;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition + new Vector2(0.0f, rectTransform.position.y);
    }

    void FixedUpdate()
    {
        if(gameObject.activeInHierarchy)
            TextScroll();
    }

    public void TextScroll()
    {
        if (startScroll)
        {
            transform.Translate(0f, scrollSpeed, 0f);

        }


        // There should be a 1500 points offset from text stop to logo.
        if (rectTransform.anchoredPosition.y >= 11200)
        {
            startScroll = false;
        }

    }

    public void OnEnable()
    {
        rectTransform.anchoredPosition = startPos;
    }

    public void SetActiveScroll(bool scrollStart)
    {
        startScroll = scrollStart;
    }
}
