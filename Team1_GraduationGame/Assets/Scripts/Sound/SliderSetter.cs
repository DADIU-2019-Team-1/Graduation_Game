// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.Sound
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class SliderSetter : MonoBehaviour
    {
        private Slider _thisSlider;
        public int uniqueId;

        private void Awake()
        {
            gameObject.GetComponent<Slider>();
        }

        private void Start()
        {
            if (_thisSlider == null)
            {
                _thisSlider = GetComponent<Slider>();
            }

            if (_thisSlider != null)
            {
                if (uniqueId == 1)
                    _thisSlider.value = PlayerPrefs.GetFloat("SFXSliderSave");
                else if (uniqueId == 2)
                    _thisSlider.value = PlayerPrefs.GetFloat("MusicSliderSave");
            }
        }

        public void SetSlider(float value)
        {
            if (_thisSlider != null)
            {
                _thisSlider.value = value;
            }
        }

        public void SetSlider(float value, int iD)
        {
            if (_thisSlider != null)
            {
                if (iD == uniqueId)
                    _thisSlider.value = value;
            }
        }
    }
}