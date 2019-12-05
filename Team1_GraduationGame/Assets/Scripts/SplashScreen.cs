using System.Collections.Generic;
using UnityEngine;
using Team1_GraduationGame.UI;

public class SplashScreen : MonoBehaviour
{
    private List<WhiteFadeController> fadeControllers;
    private bool _doOnce = false;

    void Start()
    {
        if (PlayerPrefs.GetInt("GameRanBool") == 0)
        {
            QualitySettings.SetQualityLevel(5);
        }
        // Set the Quality to the stored quality in player prefs
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality"));
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
