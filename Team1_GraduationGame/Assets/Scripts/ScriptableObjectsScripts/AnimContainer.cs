using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Animation Clip Container", menuName = "Anim Clip Container", order = 10)]
[System.Serializable]
public class AnimContainer : ScriptableObject
{
    public AnimationClip[] animationClips;
}
