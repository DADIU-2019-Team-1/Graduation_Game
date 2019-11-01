using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    [SerializeField] private Transform camTarget;
    [SerializeField] private Transform camRail;
    void LateUpdate()
    {
        //// Position update
        //transform.position = Vector3.SmoothDamp(transform.position, new Vector3(player.position.x, camRail.position.y, camRail.position.z),
        //    ref camMovement, camMoveTime.value * Time.deltaTime);


        //// Rotation update
        //lookPosition = CalculateLookPosition(player.position, camTarget.position, focusRange.value, focusObjects);
        //targetRotation = (lookPosition - transform.position != Vector3.zero)
        //    ? Quaternion.LookRotation(lookPosition - transform.position) : Quaternion.identity;
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, camLookSpeed.value * Time.deltaTime);
    }
}
