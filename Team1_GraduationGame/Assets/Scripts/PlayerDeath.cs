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

    public void Start()
    {
        fadeBlackAnimator = GetComponent<Animator>();
        spManager = FindObjectOfType<SavePointManager>();

        _player = GameObject.FindGameObjectWithTag("Player");
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
                StartCoroutine(FadeHandler());
        }
    }

    public void AnimationDone()
    {

        _allowStateChange = true;

    }
    
    
    private IEnumerator FadeHandler()
    {
        _isFading = true;

        fadeBlackAnimator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(animationDuration);

        spManager.LoadToPreviousCheckpoint();

        yield return new WaitForSeconds(0.75f);

        fadeBlackAnimator.SetTrigger("FadeIn");

        yield return new WaitForSeconds(0.75f);

        fadeBlackAnimator.SetTrigger("Reset");

        _isFading = false;
    }
}
