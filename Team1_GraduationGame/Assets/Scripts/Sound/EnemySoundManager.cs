// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.Sound
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Team1_GraduationGame.Enemies;

    [RequireComponent(typeof(AkGameObj))]
    public class EnemySoundManager : MonoBehaviour
    {
        public bool useGlobalRtpcs = false;

        // Wwise:
        public AK.Wwise.RTPC speedRTPC;
        public AK.Wwise.RTPC stateRTPC;
        public AK.Wwise.Event killingPlayerEvent;
        public AK.Wwise.Event pushedDownEvent;
        public AK.Wwise.Event gettingUpEvent;

        private Enemy _thisEnemy;

        private void Awake()
        {
            if (gameObject.GetComponent<Enemy>())
            {
                _thisEnemy = gameObject.GetComponent<Enemy>();
                InvokeRepeating("CustomUpdate", 0.3f, 0.7f);
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

        public void killingPlayer()
        {
            killingPlayerEvent?.Post(gameObject);
        }

        public void pushedDown()
        {
            pushedDownEvent?.Post(gameObject);
        }

        public void gettingUp()
        {
            gettingUpEvent?.Post(gameObject);
        }
    }
}