using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Tooltip("If Camera Target is not set, object with tag \"CamLookAt\" will be used instead. \n\"Player\" will be used as a fallback.")]
    public Transform camTarget;
    [Tooltip("If Player Target is not set, object with tag \"Player\" will be used instead.")]
    public Transform player;
    [Tooltip("If Camera Rail is not set, object with tag \"RailCamera\" will be used instead.")]
    public Transform camRail;

    [Tooltip("This value is used to determine the height when the target is far away.")] 
    [SerializeField] private FloatReference heightDistanceFactor;
    [Tooltip("A higher value makes the camera LookAt more aggressive.")]
    [SerializeField] private FloatReference camLookSpeed;
    [Tooltip("A lower value makes the camera move to the desired position faster.")]
    [SerializeField] private FloatReference camMoveTime;
    private float heightIncrease;
    private Vector3 camMovement;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        if (camTarget == null)
        {
            camTarget = GameObject.FindGameObjectWithTag("CamLookAt").transform;
            if (camTarget == null)
                camTarget = player;
        }
        if (camRail == null)
            camRail = GameObject.FindGameObjectWithTag("RailCamera").transform;
    }

    void LateUpdate()
    {
        // Position update
        heightIncrease = Vector3.Distance(player.position, new Vector3(player.position.x, camRail.position.y, camRail.position.z)) * heightDistanceFactor.value;
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(camTarget.position.x, camRail.position.y + heightIncrease, camRail.position.z),
            ref camMovement, camMoveTime.value * Time.deltaTime);

        // Rotation update
        Quaternion targetRotation = (camTarget.position - transform.position != Vector3.zero)
            ? Quaternion.LookRotation(camTarget.position - transform.position) : Quaternion.identity;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, camLookSpeed.value * Time.deltaTime);
    }
}
