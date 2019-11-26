using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class SkipCutscene : MonoBehaviour
{
    public PlayableDirector peeD;

    public void SkipButton()
    {
        peeD.time = 28;
    }

    void Update()
    {
        if (peeD.time >= 26)
        {
            gameObject.SetActive(false);
        }
    }
}
