using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHideTutorial : MonoBehaviour
{

    public GameObject uiTriggerObject;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            uiTriggerObject.SetActive(false);
    }
}
