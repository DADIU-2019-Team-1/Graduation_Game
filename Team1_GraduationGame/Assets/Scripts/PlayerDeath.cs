// Code Owner: Nicolai Hansen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team1_GraduationGame.SaveLoadSystem;
using UnityEditor;

public class PlayerDeath : MonoBehaviour
{
    public AK.Wwise.Event deathEvent;
    public Animator fadeBlackAnimator;
    public SavePointManager spManager;
    private bool _allowStateChange, _hasFaded, _isFading;
    public float animationDuration = 1.0f;
    private GameObject _player;
    private WaitForSeconds _shortWait, _animWait;

    public void Start()
    {
        fadeBlackAnimator = GetComponent<Animator>();
        spManager = FindObjectOfType<SavePointManager>();

        _player = GameObject.FindGameObjectWithTag("Player");

        _shortWait = new WaitForSeconds(0.75f);
        _animWait = new WaitForSeconds(animationDuration);
    }

    public void PlayerFallingSound()
    {
        if (_player != null)
            deathEvent?.Post(_player);
    }

    public void PlayerRespawn()
    {
        if (spManager != null)
        {
            if (!_isFading)
            {
                _isFading = true;
                StartCoroutine(FadeHandler());
            }
        }
    }

    public void AnimationDone()
    {

        _allowStateChange = true;

    }
    
    
    private IEnumerator FadeHandler()
    {
        fadeBlackAnimator.SetTrigger("FadeOut");

        yield return _animWait;

        spManager.LoadToPreviousCheckpoint();

        yield return _shortWait;

        fadeBlackAnimator.SetTrigger("FadeIn");

        yield return _shortWait;

        fadeBlackAnimator.SetTrigger("Reset");

        _isFading = false;
    }
}
