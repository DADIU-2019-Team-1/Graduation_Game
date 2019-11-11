using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    [SerializeField] private CinemachineSmoothPath _rail;
    [SerializeField] private CinemachineVirtualCamera railCamera;
    [SerializeField] private Transform[] lookAtTargets;
    [SerializeField] private GameObject[] objectsToActive;
    [SerializeField] private FloatReference camLookSpeed, camMoveTime;
    [SerializeField] private FloatReference activateMenuThreshold, waitBeforeMoving;
    [SerializeField] [Range(0.01f, 1.0f)] private float fadeAmount = 0.01f;
    private int currentTargetIndex, railIndex = 0;
    private Quaternion targetRotation;
    private Vector3 camMovement;
    private bool _move;

    void Start()
    {
        if (_rail == null)
            _rail = FindObjectOfType<CinemachineSmoothPath>();
        if (railCamera == null)
            railCamera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    void LateUpdate()
    {
        // Object activation
        if (Vector3.Distance(transform.position, _rail.m_Waypoints[railIndex].position) <= activateMenuThreshold.value)
        {
            StopCoroutine(WaitForTextFade());
            for (int i = 0; i < objectsToActive.Length; i++)
            {
                if (i == currentTargetIndex)
                {
                    objectsToActive[i].SetActive(true);
                    objectsToActive[i].GetComponent<CanvasGroup>().alpha += fadeAmount;
                }
                else
                {
                    objectsToActive[i].GetComponent<CanvasGroup>().alpha -= fadeAmount;
                    if (objectsToActive[i].GetComponent<CanvasGroup>().alpha < fadeAmount)
                        objectsToActive[i].SetActive(false);
                }
            }
        }
        else
        {
            StartCoroutine(WaitForTextFade());
            for (int i = 0; i < objectsToActive.Length; i++)
            {
                objectsToActive[i].GetComponent<CanvasGroup>().alpha -= fadeAmount;
                if (objectsToActive[i].GetComponent<CanvasGroup>().alpha < fadeAmount)
                    objectsToActive[i].SetActive(false);
            }
        }

        if (_move)
        {

            Debug.Log("MOVE IS ACTIVE");
            // Position update
            transform.position = new Vector3(transform.position.x, railCamera.transform.position.y, railCamera.transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(_rail.m_Waypoints[railIndex].position.x, transform.position.y, transform.position.z),
                ref camMovement, camMoveTime.value * Time.deltaTime);

            // Rotation update
            targetRotation = lookAtTargets[currentTargetIndex].position - transform.position != Vector3.zero
                ? Quaternion.LookRotation(lookAtTargets[currentTargetIndex].position - transform.position) : Quaternion.identity;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, camLookSpeed.value * Time.deltaTime);
        }
    }

    public void ChangeLookAt(int i)
    {
        _move = false;
        currentTargetIndex = i;
        railIndex = currentTargetIndex > 0 ? _rail.m_Waypoints.Length - 1 : 0;
    }

    IEnumerator WaitForTextFade()
    {
        yield return new WaitForSeconds(waitBeforeMoving.value);
        _move = true;
    }
}
