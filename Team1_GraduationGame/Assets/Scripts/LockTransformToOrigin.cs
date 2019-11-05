using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LockTransformToOrigin : MonoBehaviour
{
    void Update()
    {
        if (transform.position != Vector3.zero || transform.eulerAngles != Vector3.zero || transform.lossyScale != Vector3.one)
        {
            Debug.Log("A locked object was moved - Resetting Transform. Locked objects cannot be moved! Object is: " + gameObject.name + ".");
            transform.Reset();
        }
    }
}
