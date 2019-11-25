// Code Owner: Jannik Neerdal
using System;
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

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}