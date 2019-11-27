using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeQuality : MonoBehaviour
{
    public Button Qualitybutton;
    public int settingLevel;
    // Start is called before the first frame update
    void Start()
    {
        Qualitybutton.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        //Output this to console when Button1 or Button3 is clicked
        QualitySettings.SetQualityLevel(settingLevel, true);
    }
}
