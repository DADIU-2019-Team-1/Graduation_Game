using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMCam : MonoBehaviour
{
    private Vector3 offset;
    private Transform player;
    public float lerpTime;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        offset = transform.position - player.position;
    }
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, player.position + offset, lerpTime);
    }
}
