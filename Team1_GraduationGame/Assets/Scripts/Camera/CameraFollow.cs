using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Transform cam;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.position.x,cam.position.y,player.position.y);
    }
}
