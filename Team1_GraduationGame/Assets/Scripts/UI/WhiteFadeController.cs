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
        public bool delayFade;
        public float splashDuration;

        public void RaiseFadeEvent()
        {
            if (delayFade)
            {
                Debug.Log("Entered delayed fade");
                StartCoroutine(delayedFade());
            }
            else
            {
                Debug.Log("Invoked FadeEvent for gameobject " + gameObject);
                StopAllCoroutines();
                fadeEvent?.Invoke();
            }
        }

        public IEnumerator delayedFade()
        {
            yield return new WaitForSeconds(splashDuration);
            Debug.Log("Coroutine after wait");
            delayFade = false;
            fadeEvent?.Invoke();
            StopAllCoroutines();
        }
    }
}