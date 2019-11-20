using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomOffsetAnimation : MonoBehaviour
{
    private Animator animator;
    public int Offset;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        if (Offset != 0)
        {
            animator.SetTrigger("Offset" + Offset);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
