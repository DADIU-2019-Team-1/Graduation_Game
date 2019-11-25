using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsScroll : MonoBehaviour
{
    public bool startScroll;
    Vector2 startPos, scrollPos;
    public float scrollSpeed = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        //startPos = transform.position;
        startPos = GetComponent<RectTransform>().anchoredPosition;
        //scrollPos = startPos;

    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.activeInHierarchy)
            TextScroll();
    }

    public void TextScroll()
    {
        if (startScroll)
        {
            //Debug.Log("Should scroll: " + scrollPos.y);
            transform.Translate(0f, scrollSpeed, 0f);

        }

        if (GetComponent<RectTransform>().anchoredPosition.y == 3590)
        {
            startScroll = false;
        }

    }

    public void OnEnable()
    {
        GetComponent<RectTransform>().anchoredPosition = startPos;
    }

    public void SetActiveScroll(bool scrollStart)
    {
        startScroll = scrollStart;
    }
}
