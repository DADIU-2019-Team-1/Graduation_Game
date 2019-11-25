using System.Collections;
using UnityEngine;

public class DisableOnPause : MonoBehaviour
{
    private UIMenu pauseScript;
    private void Start()
    {
        if (FindObjectOfType<UIMenu>() != null)
        {
            pauseScript = FindObjectOfType<UIMenu>();
            pauseScript.gamePauseState += SetActive;
        }
    }

    private void SetActive(bool activity)
    {
        gameObject.SetActive(!activity);
    }
}
