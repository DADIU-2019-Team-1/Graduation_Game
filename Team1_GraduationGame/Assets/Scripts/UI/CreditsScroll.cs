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

    void Update()
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

        if (rectTransform.anchoredPosition.y >= 9700)
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
