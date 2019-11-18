using System.Collections;
using UnityEngine;

public class DisableOnPause : MonoBehaviour
{
    private InGameUI pauseScript;
    private void Start()
    {
        if (FindObjectOfType<InGameUI>() != null)
        {
            pauseScript = FindObjectOfType<InGameUI>();
            pauseScript.gamePauseState += SetActive;
        }
    }

    private void SetActive(bool activity)
    {
        gameObject.SetActive(!activity);
    }
}
