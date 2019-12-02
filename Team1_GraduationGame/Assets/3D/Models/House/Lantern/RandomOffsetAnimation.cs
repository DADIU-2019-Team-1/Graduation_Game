using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomOffsetAnimation : MonoBehaviour
{
    private Animator animator;
    public int Offset;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (Offset != 0)
        {
            animator.SetTrigger("Offset" + Offset);
        }
    }
}