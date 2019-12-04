using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndCredit : MonoBehaviour
{
    public void ResetSaves()
    {
        PlayerPrefs.SetInt("previousGame", 0);
    }
}
