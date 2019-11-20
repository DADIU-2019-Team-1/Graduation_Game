using System;
using System.Collections;
using System.Collections.Generic;
using Team1_GraduationGame.MotionMatching;
using UnityEngine;

public class AnimationStateHandler : StateMachineBehaviour
{
    private MotionMatching mm;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("MotionMatching"))
        {
            if (mm != null)
            {
                mm.ChangeLayerWeight(layerIndex, 0);
            }
            else
            {
                mm = FindObjectOfType<MotionMatching>();
                mm.ChangeLayerWeight(layerIndex, 0);
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("MotionMatching"))
        {
            if (mm != null)
            {
                mm.ChangeLayerWeight(layerIndex, 1);
            }
            else
            {
                mm = FindObjectOfType<MotionMatching>();
                mm.ChangeLayerWeight(layerIndex, 1);
            }
        }
    }
}
