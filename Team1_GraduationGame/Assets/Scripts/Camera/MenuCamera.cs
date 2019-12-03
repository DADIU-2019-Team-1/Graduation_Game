// Code owner: Jannik Neerdal - Optimized
using System.Collections;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

public class MenuCamera : MonoBehaviour
{
    // --- Inspector
    [SerializeField] private CinemachineSmoothPath _rail;
    [SerializeField] private CinemachineVirtualCamera railCamera;
    [SerializeField] private Transform[] lookAtTargets;
    [SerializeField] private CanvasGroup[] menuObjectsToSetActivate;
    [SerializeField] private PlayableDirector startingTimeline;
    [SerializeField] private FloatReference camLookSpeed, camMoveTime;
    [SerializeField] private FloatReference activateMenuThreshold, waitBeforeMoving;
    [SerializeField] [Range(0.01f, 1.0f)] private float fadeAmount = 0.05f;

    // --- Hidden
    private int _currentTargetIndex, 
        _railIndex = 0, 
        _currentLookAt = 0;
    private Quaternion _targetRotation;
    private Vector3 _camMovement;
    private bool _move, 
        _startingGame;

    void Start()
    {
        if (_rail == null)
            _rail = FindObjectOfType<CinemachineSmoothPath>();
        if (railCamera == null)
            railCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (startingTimeline == null)
            startingTimeline = FindObjectOfType<PlayableDirector>();

        UIMenu[] menus = Resources.FindObjectsOfTypeAll<UIMenu>(); // Find all event-sending objects, even if they are inactive
        for (int i = 0; i < menus.Length; i++)
        {
            menus[i].menuChangeEvent += ChangeLookAt; // Subscribe ChangeLookAt to the menuChangeEvent
            menus[i].startGameEvent += StartGame; // Subscribe StartGame to the startGameEvent
        }

        // If there are lookAtTargets assigned in the inspector, initialize the camera to look at the first target
        if (lookAtTargets.Length > 0)
        {
            transform.LookAt(lookAtTargets[0].position);
        }
    }

    void LateUpdate()
    {
        if (!_startingGame)
        {
            // Object activation
            if (math.distancesq(transform.position, _rail.m_Waypoints[_railIndex].position + _rail.transform.position) <= math.pow(activateMenuThreshold.value,2))
            {
                StopCoroutine(nameof(WaitForTextFade));
                for (int i = 0; i < menuObjectsToSetActivate.Length; i++)
                {
                    if (i == _currentLookAt)
                    {
                        menuObjectsToSetActivate[i].gameObject.SetActive(true);
                        menuObjectsToSetActivate[i].alpha += fadeAmount;
                    }
                    else
                    {
                        menuObjectsToSetActivate[i].alpha -= fadeAmount;
                        if (menuObjectsToSetActivate[i].alpha < fadeAmount)
                            menuObjectsToSetActivate[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                StartCoroutine(WaitForTextFade());
                for (int i = 0; i < menuObjectsToSetActivate.Length; i++)
                {
                    menuObjectsToSetActivate[i].alpha -= fadeAmount;
                    if (menuObjectsToSetActivate[i].alpha < fadeAmount)
                        menuObjectsToSetActivate[i].gameObject.SetActive(false);
                }
            }

            if (_move)
            {
                // Position update - Currently only smoothes on the x-axis
                transform.position = new Vector3(transform.position.x, railCamera.transform.position.y, railCamera.transform.position.z);
                transform.position = Vector3.SmoothDamp(transform.position, new Vector3(_rail.m_Waypoints[_railIndex].position.x + _rail.transform.position.x, transform.position.y, transform.position.z),
                    ref _camMovement, camMoveTime.value * Time.deltaTime);

                // Rotation update
                _targetRotation = lookAtTargets[_currentTargetIndex].position - transform.position != Vector3.zero
                    ? Quaternion.LookRotation(lookAtTargets[_currentTargetIndex].position - transform.position) : Quaternion.identity;
                transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, camLookSpeed.value * Time.deltaTime);
            }
        }
        else
        {
            bool startGame = false;
            for (int i = 0; i < menuObjectsToSetActivate.Length; i++)
            {
                if (menuObjectsToSetActivate[i].gameObject.activeSelf)
                {
                    menuObjectsToSetActivate[i].alpha -= fadeAmount;
                    if (menuObjectsToSetActivate[i].alpha < fadeAmount)
                    {
                        startGame = true;
                    }
                }
            }
            if (startGame)
            {
                StartGame();
            }
        }
    }

    public void ChangeLookAt(int i)
    {
        _move = false;
        _currentTargetIndex = i;
        _railIndex = _currentTargetIndex > 0 ? _rail.m_Waypoints.Length - 1 : 0;
    }

    public void StartGame()
    {
        if (_startingGame)
        {
            startingTimeline.Play();
        }
        _startingGame = true;
    }

    IEnumerator WaitForTextFade()
    {
        yield return new WaitForSeconds(waitBeforeMoving.value);
        if (!_move)
        {
            _currentLookAt = _currentTargetIndex;
            if (_currentTargetIndex > lookAtTargets.Length - 1)
            {
                _currentTargetIndex = lookAtTargets.Length - 1;
            }
        }
        _move = true;
    }
}
