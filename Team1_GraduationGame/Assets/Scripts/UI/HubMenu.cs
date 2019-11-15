// Code Owner: Jannik Neerdal
using System;
using UnityEngine;

public class HubMenu : MonoBehaviour
{
    public event Action startGameEvent;
    public event Action continueGameEvent; // TODO: Continue is not yet implemented 
    public event Action<int> menuChangeEvent;
    public event Action<string> languageChangeEvent; // TODO: Localization is not yet implemented
    public event Action cheatModeEvent;

    public void ChangeLanguage(string s)
    {
        languageChangeEvent?.Invoke(s);
    }
    public void ChangeMenu(int i)
    {
        menuChangeEvent?.Invoke(i);
    }
    public void StartGame()
    {
        startGameEvent?.Invoke();
    }
    public void ContinueGame()
    {
        continueGameEvent?.Invoke();
    }
    public void CheatMode()
    {
        cheatModeEvent?.Invoke();
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
