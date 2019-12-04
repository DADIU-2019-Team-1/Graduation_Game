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