using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Transform camRail;
    [Tooltip("This value is used to determine the height when the target is far away.")]
    public float heightDistanceFactor = 0.2f;
    private float heightIncrease;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        if (camRail == null)
            camRail = GameObject.FindGameObjectWithTag("RailCamera").transform;
    }

    // Update is called once per frame
    void Update()
    {
        heightIncrease = Vector3.Distance(player.position, new Vector3(player.position.x, camRail.position.y, camRail.position.z)) * 0.2f;
        transform.position = new Vector3(player.position.x, camRail.position.y + heightIncrease, camRail.position.z);
    }
}
