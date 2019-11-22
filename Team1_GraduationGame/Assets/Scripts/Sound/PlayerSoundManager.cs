namespace Team1_GraduationGame.Sound
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(AkGameObj))]
    public class PlayerSoundManager : MonoBehaviour
    {
        public AK.Wwise.Event jumpEvent, miniJumpEvent, attackEvent;
        public AK.Wwise.RTPC motionState;

        public void JumpEvent()
        {
            jumpEvent?.Post(gameObject);
        }
        public void MiniJumpEvent()
        {
            miniJumpEvent?.Post(gameObject);
        }
        public void AttackEvent()
        {
            attackEvent?.Post(gameObject);
        }
        public void MotionStateUpdate(int state)
        {
            if (motionState != null)
                AkSoundEngine.SetRTPCValue(motionState.Id, state);
        }
    }
}