// Codeowner: Nicolai Hansen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SphereCollider))]
public class ThomasGoToScene : MonoBehaviour
{
    public bool forceSwitch;

    // Global booleans as ints, instead of script dependencies. 0 = true, 1 = false. It's reverse, i know.
    public IntVariable atOrbTrigger;
    private SphereCollider _collider;
    private Collider _Collider;

    [SerializeField] [Range(0f,1f)]
    private float timelineThreshold;

    private Movement _movement;

    private Vector3 memoryDirection;

    private bool destinationReached;
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<SphereCollider>();
        _movement = GetComponent<Movement>();

        if(FindObjectOfType<HubMenu>() != null)
            FindObjectOfType<HubMenu>().startGameEvent += SetOrbTrigger;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (atOrbTrigger != null)
        {

            if (atOrbTrigger.value == 0 && _movement != null  && !destinationReached)
            {
                memoryDirection = (transform.position - _movement.gameObject.transform.position).normalized;
                _movement.direction = memoryDirection;
                _movement.movePlayer(memoryDirection);

                    if (Vector3.Distance(_movement.gameObject.transform.position, transform.position) <=
                         _collider.radius * timelineThreshold)
                    {
                        Debug.Log("Reached timeline");
                        _movement.targetSpeed = 0;
                        memoryDirection = Vector3.zero;
                        //destinationReached = true;
                        atOrbTrigger.value = 1;
                        destinationReached = true;

                        if (SceneManager.GetActiveScene().name.Contains("mem"))
                        {
                            MemoryTimeLineEnded();
                        }
                    }
            }
        }

        
    }
    public void GoToSceneWithName(string name)
    { 
        
        Debug.Log("going to scene '" +name+"'");
        SceneManager.LoadScene(name);
        destinationReached = false;
        _movement.Frozen(false);
        //movingToOrb.value = 1;
    } 
    
    private void OnTriggerEnter(Collider other)
    {
        _movement = other.GetComponent<Movement>();
        if (forceSwitch && other.tag == "Player")
        {
            _movement.Frozen(true);
            //atOrbTrigger.value = 0;
            //movingToOrb.value = 0;
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

    public void SetOrbTrigger()
    {
        if(atOrbTrigger != null) 
            atOrbTrigger.value = 1;
    }
}


