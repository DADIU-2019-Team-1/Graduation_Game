using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team1_GraduationGame.UI;
using UnityEngine.Rendering;

public class SplashScreen : MonoBehaviour
{
    // Start is called before the first frame update
    private List<WhiteFadeController> fadeControllers;
    private bool _doOnce = false;
    void Start()
    {
        fadeControllers = new List<WhiteFadeController>();
        fadeControllers.Add(GetComponent<WhiteFadeController>());
        fadeControllers.AddRange(GetComponentsInChildren<WhiteFadeController>());
        for (int i = 0; i < fadeControllers.Count; i++)
        {
            if (fadeControllers[i] != null)
            {
                fadeControllers[i].RaiseFadeEvent();
            }
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && fadeControllers[0] != null && UnityEngine.Rendering.SplashScreen.isFinished && !_doOnce)
        {
            fadeControllers[0].StopAllCoroutines();
            fadeControllers[0].fadeEvent?.Invoke();
            _doOnce = true;
        }
    }
}
