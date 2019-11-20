using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class StartCutscenesFromTrigger : MonoBehaviour
{

    public Collider _Collider;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        gameObject.GetComponentInChildren<PlayableDirector>().Play();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
