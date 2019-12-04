// Code Owner: Jannik Neerdal - Optimized
using System;
using System.Collections;
using UnityEngine;

public class UIMenu : MonoBehaviour
{
    [SerializeField] private float _fadeAmount = 0.05f;

    public event Action startGameEvent,
        continueGameEvent,
        menuButtonPressEvent,
        cheatModeEvent;
    public event Action<int> menuChangeEvent,
        languageChangeEvent;
    public event Action<float> musicSliderEvent,
        sfxSliderEvent;
    public event Action<bool> gamePauseState;
    private CanvasGroup _groupToFade;
    private bool _fading,
        _continueQueued;
    private WaitForSecondsRealtime fixedDelayTime = new WaitForSecondsRealtime(0.02f);
    
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
        if (group.gameObject.activeSelf == false)
        {
            group.gameObject.SetActive(true);
        }

        if (fadeIn)
        {
            group.alpha = 0.00f;
        }
        else
        {
            group.alpha = 1.00f;
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

            yield return fixedDelayTime;
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

        if (_continueQueued)
        {
            ContinueGame();
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
        if (_fading)
        {
            _continueQueued = true;
        }
        else
        {
            continueGameEvent?.Invoke();
        }
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