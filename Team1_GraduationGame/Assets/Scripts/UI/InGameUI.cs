// Code Owner: Jannik Neerdal
using System;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    public event Action<bool> gamePauseState; 
    public void PauseGame(bool pause)
    {
        Time.timeScale = pause ? 0 : 1;
        gamePauseState?.Invoke(pause);
    }
}
