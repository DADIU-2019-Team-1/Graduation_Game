// Code Owner: Jannik Neerdal
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ChangeQuality : MonoBehaviour
{
    [SerializeField] private int settingLevel;
    public event Action<int> changeQuality;
    private Button _thisBtn;
    private ChangeQuality[] _qualityBtns;
    public GameObject[] terrains;



    private void Awake()
    {
        _thisBtn = GetComponent<Button>();
        _qualityBtns = Resources.FindObjectsOfTypeAll<ChangeQuality>();
        for (int i = 0; i < _qualityBtns.Length; i++)
        {
            _qualityBtns[i].changeQuality += QualityChangeHandler;
        }
        QualityChangeHandler(PlayerPrefs.GetInt("Quality"));
    }

    public void ChangeQualityEvent()
    {
        changeQuality?.Invoke(settingLevel);
        QualitySettings.SetQualityLevel(settingLevel, true);
        // Set terrain to not draw grass if settingLevel at 0
        if(settingLevel == 0)
        {
            if( terrains != null)
            {
                foreach(GameObject terrainVar in terrains)
                {
                    terrainVar.GetComponent<Terrain>().drawTreesAndFoliage = false;
                }

            
            }
        }
        // Set terrain to do draw grass if settingLevel higher than 0
        else 
        {
            if( terrains != null)
            {
                foreach(GameObject terrainVar in terrains)
                {
                    terrainVar.GetComponent<Terrain>().drawTreesAndFoliage = true;
                }

            
            }
        }


        QualityChangeHandler(settingLevel);
        PlayerPrefs.SetInt("Quality", settingLevel);
    }

    private void QualityChangeHandler(int setting)
    {
        if (setting == settingLevel)
        {
            _thisBtn.interactable = false; // Button is of the same language as the event, so it should not be pressed
        }
        else
        {
            _thisBtn.interactable = true; // Button is of a language as the event, so it should not be pressed
        }
    }
}