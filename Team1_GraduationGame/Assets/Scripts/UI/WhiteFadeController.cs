// Script by Jakob Elkjær Husted - Optimized by Jannik Neerdal
namespace Team1_GraduationGame.UI
{
    using System.Collections;
    using UnityEngine.Events;
    using UnityEngine;

    public class WhiteFadeController : MonoBehaviour
    {
        public UnityEvent fadeEvent;
        public bool delayFade;
        public float splashDuration;
        private WaitForSeconds _delayedFadeTime;

        private void Awake()
        {
            _delayedFadeTime = new WaitForSeconds(splashDuration);
        }

        public void RaiseFadeEvent()
        {
            if (delayFade)
            {
                StartCoroutine(DelayedFade());
            }
            else
            {
                StopAllCoroutines();
                fadeEvent?.Invoke();
            }
        }

        public IEnumerator DelayedFade()
        {
            yield return _delayedFadeTime;
            delayFade = false;
            fadeEvent?.Invoke();
            StopAllCoroutines();
        }
    }
}