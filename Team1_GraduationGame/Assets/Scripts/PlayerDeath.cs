using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team1_GraduationGame.SaveLoadSystem;
using UnityEditor;

public class PlayerDeath : MonoBehaviour
{
    public Animator fadeBlackAnimator;
    public SavePointManager spManager;
    private bool _allowStateChange;
    public void Start()
    {
        fadeBlackAnimator = GetComponent<Animator>();
        
    }

    public void PlayerRespawn()
    {
        StartCoroutine("FadeHandler");


    }

    

    public void AnimationDone() {
        _allowStateChange = true;

    }
    
    
    private IEnumerator FadeHandler()
    {
        Debug.Log("Entered coroutine");
        fadeBlackAnimator.SetTrigger("FadeOut");
        if (_allowStateChange && (fadeBlackAnimator.GetCurrentAnimatorStateInfo(0).IsName("FadeIn") || fadeBlackAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle")))
        {
            Debug.Log("Entered first if FadeHandler");
            spManager.LoadToPreviousCheckpoint();
            _allowStateChange = false;
        }



        if (_allowStateChange && fadeBlackAnimator.GetCurrentAnimatorStateInfo(0).IsName("FadeOut"))
        {
            Debug.Log("Entered Second if FadeHandler");
            fadeBlackAnimator.SetTrigger("FadeIn");
            _allowStateChange = false;
            yield return null;
        }
        
    }
}
