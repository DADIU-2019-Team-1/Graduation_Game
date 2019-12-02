using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class StartCutscenesFromTrigger : MonoBehaviour
{
    //private Collider collider;
    public GameObject Target;

    //void Start()
    //{
    //    collider = GetComponent<BoxCollider>();
    //}
    private void OnTriggerEnter(Collider collider)
    {
        Target?.SetActive(true);
    }
}