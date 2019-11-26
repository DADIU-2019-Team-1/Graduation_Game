// Code Owner: Jannik Neerdal
using System;
using System.Collections;
using UnityEngine;

public class UIMenu : MonoBehaviour
{
    public event Action startGameEvent;
    public event Action continueGameEvent; // TODO: Continue is not yet implemented 
    public event Action menuButtonPressEvent;
    public event Action cheatModeEvent;
    public event Action<int> menuChangeEvent;
    public event Action<int> languageChangeEvent;
    public event Action<float> musicSliderEvent;
    public event Action<float> sfxSliderEvent;
    public event Action<bool> gamePauseState;
    private CanvasGroup _groupToFade;
    [SerializeField] private float _fadeAmount = 0.05f;
    private bool _fading;

    public void FadeOut(CanvasGroup group)
    {
        if (_fading)
        {
            _groupToFade = group;
        }
        else
        {
            _fading = true;
            StartCoroutine(HandleFade(group, false));
        }
    }
    public void FadeIn(CanvasGroup group)
    {
        if (_fading)
        {
            _groupToFade = group;
        }
        else
        {
            _fading = true;
            StartCoroutine(HandleFade(group, true));
        }
    }
    IEnumerator HandleFade(CanvasGroup group, bool fadeIn)
    {
        if (fadeIn)
        {
            group.gameObject.SetActive(true);
        }
        do // We use do-while to run the loop once before checking the condition
        {
            if (fadeIn)
            {
                group.alpha += _fadeAmount;
            }
            else
            {
                group.alpha -= _fadeAmount;
            }

            yield return new WaitForSecondsRealtime(0.02f);
        } while (group.alpha != 1.00f && group.alpha != 0.00f);
        if (!fadeIn)
        {
            group.gameObject.SetActive(false);
        }

        _fading = false;
        if (_groupToFade)
        {
            CanvasGroup tempGroup = _groupToFade;
            _groupToFade = null;
            StopCoroutine(nameof(HandleFade));
            if (fadeIn)
            {
                FadeOut(tempGroup);
            }
            else
            {
                FadeIn(tempGroup);
            }
        }
    }

    public void ChangeLanguage(int languageIndex)
    {
        languageChangeEvent?.Invoke(languageIndex);
        menuButtonPressEvent?.Invoke();
        PlayerPrefs.SetInt("Language", languageIndex);
    }
    public void ChangeMenu(int i)
    {
        menuChangeEvent?.Invoke(i);
        menuButtonPressEvent?.Invoke();
    }
    public void StartGame()
    {
        startGameEvent?.Invoke();
        menuButtonPressEvent?.Invoke();
    }
    public void ContinueGame()
    {
        continueGameEvent?.Invoke();
        menuButtonPressEvent?.Invoke();
    }
    public void PauseGame(bool pause)
    {
        Time.timeScale = pause ? 0 : 1;
        gamePauseState?.Invoke(pause);
        menuButtonPressEvent?.Invoke();
    }
    public void CheatMode()
    {
        cheatModeEvent?.Invoke();
        menuButtonPressEvent?.Invoke();
    }
    public void ChangeMusicSlider(float value)
    {
        musicSliderEvent?.Invoke(value);
    }
    public void ChangeSFXSlider(float value)
    {
        sfxSliderEvent?.Invoke(value);
    }

    public void MenuButtonPress()
    {
        menuButtonPressEvent?.Invoke();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}