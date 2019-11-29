// Codeowner: Nicolai Hansen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Team1_GraduationGame.SaveLoadSystem;
using UnityEngine.Playables;

[RequireComponent(typeof(SphereCollider))]
public class ThomasGoToScene : MonoBehaviour
{
    public bool forceSwitch;

    // Global booleans as ints, instead of script dependencies. 0 = true, 1 = false. It's reverse, i know.
    private SphereCollider _collider;
    private Collider _Collider;

    [SerializeField] [Range(0f,1f)]
    private float timelineThreshold;

    private Movement _movement;

    private Vector3 memoryDirection;

    private bool destinationReached = false;


    public BoolVariable atOrbTrigger;
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<SphereCollider>();
        _movement = GetComponent<Movement>();

        //if(FindObjectOfType<HubMenu>() != null)
        //    FindObjectOfType<HubMenu>().startGameEvent += SetOrbTrigger;

        atOrbTrigger.value = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (atOrbTrigger != null)
        {

            if (atOrbTrigger.value && _movement != null  && !destinationReached)
            {
                memoryDirection = (transform.position - _movement.gameObject.transform.position).normalized;
                _movement.direction = memoryDirection;
                _movement.movePlayer(memoryDirection);
                if (Vector3.Distance(_movement.gameObject.transform.position, transform.position) <=
                         _collider.radius * timelineThreshold)
                    {
                        _movement.targetSpeed = 0;
                        memoryDirection = Vector3.zero;
                        // Removed to test push frozen in orb range.
                        //atOrbTrigger.value = false;
                        
                        destinationReached = true;
                        //if(transform.GetChild(2).GetComponent<PlayableDirector>() != null)
                        //    transform.GetChild(2).GetComponent<PlayableDirector>().Play();

                        //Debug.Log("Playable Director child: " + gameObject.GetComponentInChildren<PlayableDirector>().name);
                        if (gameObject.GetComponentInChildren<PlayableDirector>() != null)
                        {
                            gameObject.GetComponentInChildren<PlayableDirector>().Play();
                        }

                        //if (SceneManager.GetActiveScene().name.Contains("mem"))
                        //{
                        //    MemoryTimeLineEnded();
                        //}
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
        _movement.inSneakZone = false;
        //movingToOrb.value = 1;
    } 
    
    private void OnTriggerEnter(Collider other)
    {
        if (other is CapsuleCollider)
        {
            if (forceSwitch && other.tag == "Player")
            {
                
                _movement = other.GetComponent<Movement>();
                if (_movement != null)
                {
                    atOrbTrigger.value = true;
                    _movement.Frozen(true);
                }

                //Director.SetActive(true);
                
                //movingToOrb.value = 0;
                //if (Vector3.Distance(transform.position, other.transform.position) <= collider.radius * timelineThreshold)
                //{
                //    _movement.targetSpeed = 0;
                //    memoryDirection = Vector3.zero;

                //    // Start timeline
                //}
            }
        }


    }

    public void MemoryTimeLineEnded()
    {        
        destinationReached = false;
        if(_movement != null) {
            _movement.Frozen(false);
            _movement.inSneakZone = false;
        }

        if(FindObjectOfType<SavePointManager>() != null)
            FindObjectOfType<SavePointManager>().NextLevel();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void SetOrbTrigger()
    {
        if (atOrbTrigger != null)
        {
            atOrbTrigger.value = false;
            //Debug.Log("Orb has been reset to: " + atOrbTrigger.value);
        }

    }
}


