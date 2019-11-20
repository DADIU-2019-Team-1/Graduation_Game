using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class StartCutscenesFromTrigger : MonoBehaviour
{

    public Collider Collider;
    public GameObject Target;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider Collider)
    {
        Debug.Log("Fired event");
        Target.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
