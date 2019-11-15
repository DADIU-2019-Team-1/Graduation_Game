using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SphereCollider))]
public class ThomasGoToScene : MonoBehaviour
{
    public bool forceSwitch;

    public IntVariable atOrbTrigger;
    private SphereCollider _collider;

    [SerializeField] [Range(0f,1f)]
    private float timelineThreshold;

    private Movement _movement;

    private Vector3 memoryDirection;
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<SphereCollider>();
        _movement = GetComponent<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (atOrbTrigger.value == 0 && _movement != null)
        {
            memoryDirection = (transform.position - _movement.gameObject.transform.position).normalized;
            _movement.direction = memoryDirection;
            _movement.movePlayer(memoryDirection);
            Debug.Log("Direction is: " + memoryDirection);
            if (Vector3.Distance(_movement.gameObject.transform.position, transform.position) <=
                 _collider.radius * timelineThreshold)
            {
                Debug.Log("Reached timeline");
                _movement.targetSpeed = 0;
                memoryDirection = Vector3.zero;
            }
        }
        
    }
    public void GoToSceneWithName(string name)
    { 
        atOrbTrigger.value = 1;
        Debug.Log("going to scene '" +name+"'");
        SceneManager.LoadScene(name);
    } 
    
    private void OnTriggerEnter(Collider other)
    {
        _movement = other.GetComponent<Movement>();
        if (forceSwitch && other.tag == "Player")
        {
            _movement.Frozen(true);
            atOrbTrigger.value = 0;
            //if (Vector3.Distance(transform.position, other.transform.position) <= collider.radius * timelineThreshold)
            //{
            //    _movement.targetSpeed = 0;
            //    memoryDirection = Vector3.zero;

            //    // Start timeline
            //}
        }

    }

    public void MemoryTimeLineEnded()
    {
        GoToSceneWithName("Mem01");
    }
}


