using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseGame : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Time.timeScale = 0;

        }

        if(Input.GetKeyDown(KeyCode.KeypadEnter)) {
            Time.timeScale = 1;
        }
    }
}
