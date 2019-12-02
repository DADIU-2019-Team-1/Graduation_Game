using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomOffsetAnimation : MonoBehaviour
{
    private Animator animator;
    public int Offset;

    private void Start()
    {
        Invoke("DelayedStart", PlayerPrefs.GetInt("loadGameOnAwake") == 1 ? 0.0f : 2.5f);
    }

    private void DelayedStart()
    {
        animator = GetComponent<Animator>();
        if (Offset != 0)
        {
            animator.SetTrigger("Offset" + Offset);
        }
    }
}