// Code owner: Jannik Neerdal
using System.Collections;
using Cinemachine;
using Team1_GraduationGame.SaveLoadSystem;
using UnityEngine;
using UnityEngine.Playables;

public class MenuCamera : MonoBehaviour
{
    [SerializeField] private CinemachineSmoothPath _rail;
    [SerializeField] private CinemachineVirtualCamera railCamera;
    [SerializeField] private Transform[] lookAtTargets;
    [SerializeField] private CanvasGroup[] menuObjectsToSetActivate;
    [SerializeField] private PlayableDirector startingTimeline;
    [SerializeField] private FloatReference camLookSpeed, camMoveTime;
    [SerializeField] private FloatReference activateMenuThreshold, waitBeforeMoving;
    [SerializeField] [Range(0.01f, 1.0f)] private float fadeAmount = 0.05f;
    private int currentTargetIndex, railIndex = 0;
    private Quaternion targetRotation;
    private Vector3 camMovement;
    private bool _move, _startingGame;

    void Start()
    {
        if (_rail == null)
            _rail = FindObjectOfType<CinemachineSmoothPath>();
        if (railCamera == null)
            railCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (startingTimeline == null)
            startingTimeline = FindObjectOfType<PlayableDirector>();

            FindObjectOfType<HubMenu>().menuChangeEvent += ChangeLookAt;
        FindObjectOfType<HubMenu>().startGameEvent += StartGame;
    }

    void LateUpdate()
    {
        if (!_startingGame)
        {
            // Object activation
            if (Vector3.Distance(transform.position, _rail.m_Waypoints[railIndex].position + _rail.transform.position) <= activateMenuThreshold.value)
            {
                StopCoroutine(WaitForTextFade());
                for (int i = 0; i < menuObjectsToSetActivate.Length; i++)
                {
                    if (i == currentTargetIndex)
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
                // Position update // TODO: Optimize so not only smoothening x axis (or smoothening all)
                transform.position = new Vector3(transform.position.x, railCamera.transform.position.y, railCamera.transform.position.z);
                transform.position = Vector3.SmoothDamp(transform.position, new Vector3(_rail.m_Waypoints[railIndex].position.x + _rail.transform.position.x, transform.position.y, transform.position.z),
                    ref camMovement, camMoveTime.value * Time.deltaTime);

                // Rotation update
                targetRotation = lookAtTargets[currentTargetIndex].position - transform.position != Vector3.zero
                    ? Quaternion.LookRotation(lookAtTargets[currentTargetIndex].position - transform.position) : Quaternion.identity;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, camLookSpeed.value * Time.deltaTime);
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
        currentTargetIndex = i;
        railIndex = currentTargetIndex > 0 ? _rail.m_Waypoints.Length - 1 : 0;
    }

    public void StartGame()
    {
        if (_startingGame)
        {
            SaveLoadManager man = new SaveLoadManager();
            man.NewGame();
            startingTimeline.Play();
        }
        _startingGame = true;
    }

    IEnumerator WaitForTextFade()
    {
        yield return new WaitForSeconds(waitBeforeMoving.value);
        _move = true;
    }
}
