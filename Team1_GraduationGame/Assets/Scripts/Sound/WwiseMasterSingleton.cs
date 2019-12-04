using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseMasterSingleton : MonoBehaviour
{
    private void Start()
    {
        GameObject[] _wwiseMasters = GameObject.FindGameObjectsWithTag("Wwise_Master");

        if (_wwiseMasters != null)
        {
            if (_wwiseMasters.Length > 1)
            {
                Destroy(gameObject);
            }
        }
    }
}