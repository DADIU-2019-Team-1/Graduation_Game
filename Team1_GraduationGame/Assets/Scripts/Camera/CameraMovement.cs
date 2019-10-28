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
    public float heightDistanceFactor = 0.2f;
    private float heightIncrease;

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

    void Update()
    {
        heightIncrease = Vector3.Distance(player.position, new Vector3(player.position.x, camRail.position.y, camRail.position.z)) * heightDistanceFactor;
        transform.position = new Vector3(camTarget.position.x, camRail.position.y + heightIncrease, camRail.position.z);
        transform.LookAt(camTarget); // TODO: Apply smoothing to the LookAt
    }
}
