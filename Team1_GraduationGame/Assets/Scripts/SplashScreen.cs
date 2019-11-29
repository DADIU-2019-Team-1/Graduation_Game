using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team1_GraduationGame.UI;
using UnityEngine.Rendering;

public class SplashScreen : MonoBehaviour
{
    // Start is called before the first frame update
    private WhiteFadeController fadeCtrl;
    private bool _doOnce = false;
    void Start()
    {
        fadeCtrl = GetComponent<WhiteFadeController>();
        if (fadeCtrl != null)
        {
            Debug.Log("Found fade controller");
            fadeCtrl.RaiseFadeEvent();
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && fadeCtrl != null && UnityEngine.Rendering.SplashScreen.isFinished && !_doOnce)
        {
            Debug.Log("Clicked during splash screen");
            fadeCtrl.StopAllCoroutines();
            fadeCtrl.fadeEvent?.Invoke();
            _doOnce = true;
            Debug.Log("Should stop coroutine");
        }
    }
}
