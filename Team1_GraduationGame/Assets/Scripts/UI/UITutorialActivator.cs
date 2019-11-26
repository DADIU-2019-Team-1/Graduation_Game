using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITutorialActivator : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject uiTutorialElement;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            uiTutorialElement.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
            uiTutorialElement.SetActive(false);
    }
}
