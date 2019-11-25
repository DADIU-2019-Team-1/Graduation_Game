// Script by Jakob Elkjær Husted
using System.Collections;
using Team1_GraduationGame.Enemies;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Team1_GraduationGame.Interaction
{
    public class Interactable : MonoBehaviour
    {
        // References:
        public Enemy thisEnemy;
        private Enemy[] _allEnemies;
        private GameObject _player;
        private Animator _animator;
        private ObjectPush _objectPush;
        public FloatVariable interactionDistance;
        public FloatVariable interactionAngle;

        // Public:
        public float minDistance = 2.0f, angle = 90.0f, soundEmitDistance, interactCooldown = 1.5f, minVelocityMagnitude = 0.005f;
        public bool interactableOnce, pushable, useEvents, useAnimation, switchBetweenAnimations, animationState, playSound, 
            interactConditions, checkForObstructions, emitSound, useCooldown, checkIsMoving;
        public UnityEvent eventOnInteraction;
        public string animationDefault, animationAction;
        [HideInInspector] public bool toggleState = false;

        // Wwise:
        public AK.Wwise.Event soundEvent;

        // Private:
        private bool _isEnemy, _interacted;
        private int _layerMask;

        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _layerMask = ~LayerMask.GetMask("Enemies");

            if (GetComponent<Enemy>() != null)
            {
                _isEnemy = true;
                emitSound = false;
                thisEnemy = GetComponent<Enemy>();
            }
            else
            {
                _allEnemies = FindObjectsOfType<Enemy>();
            }

            if (GetComponent<Animator>() != null)
            {
                _animator = GetComponent<Animator>();
            }

            if (GetComponent<ObjectPush>() != null)
            {
                _objectPush = GetComponent<ObjectPush>();
            }

            if (interactionDistance != null)
            {
                minDistance = interactionDistance.value;
            }

            if (interactionAngle != null)
            {
                angle = interactionAngle.value;
            }

            if (checkIsMoving && _objectPush != null)
            {
                if (soundEvent != null)
                    _objectPush.pushSoundEvent = soundEvent;
            }
        }

        public void Interact()
        {
            if (interactableOnce && toggleState)
                return;

            if (!_interacted)
            {
                if (pushable)
                {
                    PushAction();
                }
                else
                {
                    DoAction(0);
                }

                if (useCooldown && !interactableOnce)
                {
                    StartCoroutine(InteractionCooldown());
                }
            }
        }

        /// <summary>
        /// Do interaction action - Must be called from Interact()
        /// </summary>
        /// <param name="dir">0 = No direction, 1 = forward, 2 = backwards</param>
        private void DoAction(int dir)
        {
            if (useEvents)
                eventOnInteraction.Invoke();

            if (pushable)
            {
                if (_isEnemy)
                    thisEnemy.PushDown();
                else if (_objectPush != null)
                {
                    _objectPush.Push(false, dir);
                }
            }

            if (useAnimation && _animator != null)
            {
                if (switchBetweenAnimations)
                {
                    _animator.Play(animationState ? animationDefault : animationAction);
                    animationState = !animationState;
                }
                else
                {
                    _animator.Play(animationAction);
                }
            }
            else if (useAnimation && _animator == null)
            {
                Debug.LogError("Interaction Error: Animator missing on " + gameObject.name);
            }

            if (playSound)
            {
                if (checkIsMoving)
                {
                    // Do nothing, as the sound event is then on ObjectPush script
                }
                else
                {
                    soundEvent?.Post(gameObject);
                }
            }

            if (emitSound)
                HearingCheck();

            if (interactableOnce)
                _interacted = true;

            toggleState = true; // Sets the state to have been toggled (For event system)
        }

        private void PushAction()
        {
            Vector3 dir = _player.transform.position - transform.position;
            RaycastHit hit;

            if (thisEnemy == null)
            {
                if (_objectPush.wayPoints.Count <= 2)
                {
                    float thisToPlayerAngle1 = Vector3.Angle(_objectPush.wayPoints[0].transform.position - _objectPush.wayPoints[1].transform.position, dir);
                    float thisToPlayerAngle2 = Vector3.Angle(_objectPush.wayPoints[1].transform.position - _objectPush.wayPoints[0].transform.position, dir);

                    if (!interactConditions)
                    {
                        if (thisToPlayerAngle1 < 89)
                            DoAction(2);
                        else if (thisToPlayerAngle2 < 89)
                            DoAction(1);
                    }
                    else if (checkForObstructions)
                    {
                        if (Vector3.Distance(transform.position, _player.transform.position) < minDistance
                            && Physics.Raycast(transform.position + transform.up, dir, out hit, minDistance, _layerMask))
                        {
                            if (thisToPlayerAngle1 < angle / 2)
                                DoAction(2);
                            else if (thisToPlayerAngle2 < angle / 2)
                                DoAction(1);
                        }
                    }
                    else
                    {
                        if (Vector3.Distance(transform.position, _player.transform.position) < minDistance)
                        {
                            if (thisToPlayerAngle1 < angle / 2)
                                DoAction(2);
                            else if (thisToPlayerAngle2 < angle / 2)
                                DoAction(1);
                        }
                    }

                }
                else
                {
                    Debug.Log("Interaction Push Error: Please attach to waypoints using the 'Add Waypoint' button on " + gameObject.name);
                }

            }
            else if (thisEnemy != null)
            {
                if (!interactConditions)
                {
                    DoAction(0);
                }
                else if (checkForObstructions)
                {
                    if (Vector3.Distance(transform.position, _player.transform.position) < minDistance
                        && Physics.Raycast(transform.position + transform.up, dir, out hit, minDistance,
                            _layerMask))
                    {
                        DoAction(0);
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, _player.transform.position) < minDistance)
                    {
                        DoAction(0);
                    }
                }
            }
        }

        private void HearingCheck()
        {
            for (int i = 0; i < _allEnemies.Length; i++)
            {
                if (!_allEnemies[i].GetHearing() && !_allEnemies[i].alwaysAggro)
                {
                    NavMeshPath path = new NavMeshPath();
                    _allEnemies[i].getNavMeshAgent().CalculatePath(transform.position, path);

                    Vector3[] allPathPoints = new Vector3[path.corners.Length + 2];
                    allPathPoints[0] = _allEnemies[i].gameObject.transform.position;
                    allPathPoints[allPathPoints.Length - 1] = transform.position;

                    for (int j = 0; j < path.corners.Length; j++)
                    {
                        allPathPoints[j + 1] = path.corners[j];
                    }

                    float tempPathLength = 0.0f;

                    for (int j = 0; j < allPathPoints.Length - 1; j++)
                    {
                        tempPathLength += Vector3.Distance(allPathPoints[j], allPathPoints[j + 1]);
                    }

                    if (tempPathLength < soundEmitDistance && soundEmitDistance > 0.0f)
                    {
                        _allEnemies[i].SetLastSighting(_player.transform.position);
                        _allEnemies[i].SetAggro(true);
                    }
                }
            }
        }

        private IEnumerator InteractionCooldown()
        {
            _interacted = true;
            yield return new WaitForSeconds(interactCooldown);
            _interacted = false;
        }
    }
}