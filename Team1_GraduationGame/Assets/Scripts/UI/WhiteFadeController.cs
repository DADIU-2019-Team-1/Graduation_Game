// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine.Events;
    using UnityEngine;

    public class WhiteFadeController : MonoBehaviour
    {
        public UnityEvent fadeEvent;

        public void RaiseFadeEvent()
        {
            fadeEvent?.Invoke();
        }
    }
}