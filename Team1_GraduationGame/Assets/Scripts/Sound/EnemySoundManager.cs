// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.Sound
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Team1_GraduationGame.Enemies;

    public class EnemySoundManager : MonoBehaviour
    {
        public bool useGlobalRtpcs = false;

        // Wwise:
        public AK.Wwise.RTPC speedRTPC;
        public AK.Wwise.RTPC stateRTPC;

        private Enemy _thisEnemy;

        private void Awake()
        {
            if (gameObject.GetComponent<Enemy>())
            {
                _thisEnemy = gameObject.GetComponent<Enemy>();
                InvokeRepeating("CustomUpdate", 1.0f, 0.5f);
            }
        }

        private void CustomUpdate()
        {
            if (speedRTPC != null)
            {
                if (useGlobalRtpcs)
                    speedRTPC.SetGlobalValue(_thisEnemy.GetSpeed());
                else
                    speedRTPC.SetValue(gameObject, _thisEnemy.GetSpeed());
            }
            if (stateRTPC != null)
            {
                if (useGlobalRtpcs)
                    stateRTPC.SetGlobalValue(_thisEnemy.GetState());
                else
                    stateRTPC.SetValue(gameObject,_thisEnemy.GetState());
            }
        }
    }
}