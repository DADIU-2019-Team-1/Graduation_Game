using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMemoryAnimation : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        animator.SetBool("InMemory", true);
    }
}
