using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class StartCutscenesFromTrigger : MonoBehaviour
{

    private Collider collider;
    public GameObject Target;
    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<BoxCollider>();
    }
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Fired event");
        Target.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
