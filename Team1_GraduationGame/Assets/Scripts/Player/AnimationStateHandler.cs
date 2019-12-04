// Code Owner: Jannik Neerdal
using Team1_GraduationGame.MotionMatching;
using UnityEngine;

public class AnimationStateHandler : StateMachineBehaviour
{
    private MotionMatching _mm;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("MotionMatching"))
        {
            if (_mm != null)
            {
                _mm.ChangeLayerWeight(layerIndex, 0);
            }
            else
            {
                _mm = FindObjectOfType<MotionMatching>();
                _mm.ChangeLayerWeight(layerIndex, 0);
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("MotionMatching"))
        {
            if (_mm != null)
            {
                _mm.ChangeLayerWeight(layerIndex, 1);
            }
            else
            {
                _mm = FindObjectOfType<MotionMatching>();
                _mm.ChangeLayerWeight(layerIndex, 1);
            }
        }
    }
}
