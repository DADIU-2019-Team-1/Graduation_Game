// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.Enemies
{
    using System.Collections;
    using System.Collections.Generic;
    using Team1_GraduationGame.Enemies;
    using Team1_GraduationGame.Events;
    using Team1_GraduationGame.Sound;
    using UnityEngine;

    public class Big : MonoBehaviour
    {
        //References:
        private GameObject _player;
        private Movement _playerMovement;
        private Animator _playerAnimator;
        public EnemySoundManager enemySoundManager;
        private Animator _animator;
        public Light fieldOfViewLight;
        public VoidEvent playerDiedEvent;
        public GameObject visionGameObject;

        // Private:
        private LayerMask _layerMask;
        private bool _active, _isAggro, _isSpawned, _isRotating, _playerSpotted, _lightOn, _timerRunning, _isChangingState, _returnAnim;
        private int _currentSpawnPoint = 0;
        private Quaternion _lookRotation, _defaultRotation;

        // Public:
        public bool drawGizmos = true;
        public float spawnActivationDistance = 25.0f, fieldOfView = 65.0f, viewDistance = 20.0f, changeStateTime = 3.0f, aggroTime = 2.0f;
        public Color normalConeColor = Color.yellow, aggroConeColor = Color.red;
        public float animAttackTime = 3.0f;


        private void Awake()
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                _player = GameObject.FindGameObjectWithTag("Player");
                _playerAnimator = _player.GetComponent<Animator>();
                _playerMovement = _player.GetComponent<Movement>();
            }

            if (GetComponent<Animator>() != null)
            {
                _animator = GetComponent<Animator>();
            }
            else
            {
                _active = false;
                Debug.LogError("Big Sis Error: No animator on " + gameObject.name);
            }

            _layerMask = LayerMask.GetMask("Enemies");
            _layerMask |= LayerMask.GetMask("Ignore Raycast");
            _layerMask = ~_layerMask;

            if (fieldOfViewLight != null)
            {
                fieldOfViewLight.range = viewDistance;
                fieldOfViewLight.spotAngle = fieldOfView;
            }

            _defaultRotation = transform.rotation;

            if (visionGameObject != null)
                _active = true;
            else
                Debug.LogError("Big Enemy Error: Vision gameobject missing, please attach one!");
        }

        private void Start()
        {
            InvokeRepeating("DistanceCheckerLoop", 0.3f, 1.0f);
        }

        private void FixedUpdate()
        {
            if (_active)
            {
                if (_isSpawned)
                {
                    if (!_lightOn)
                        UpdateFOVLight(true, false);

                    if (!_playerSpotted)
                    {
                        if (_player != null)
                        {
                            Vector3 dir = _player.transform.position - visionGameObject.transform.position;
                            float enemyToPlayerAngle = Vector3.Angle(visionGameObject.transform.forward, dir);

                            if (enemyToPlayerAngle < (fieldOfView + 4.0f) / 2.0f)
                            {
                                RaycastHit hit;

                                if (Physics.Raycast(visionGameObject.transform.position + transform.up, dir, out hit, viewDistance, _layerMask))
                                    if (hit.collider.tag == _player.tag)
                                    {
                                        _active = false;

                                        if (!_isAggro)
                                            StartCoroutine(Aggro());

                                        if (_lightOn)
                                            UpdateFOVLight(true, true);
                                    }
                            }
                        }
                    }
                }
                else
                {
                    if (_lightOn)
                        UpdateFOVLight(false, false);
                }

            }

            if (_isAggro && !_active)
            {
                Vector3 dir = _player.transform.position - transform.position;

                Quaternion rot = Quaternion.LookRotation(_player.transform.position - transform.position) != Quaternion.identity ? Quaternion.LookRotation(_player.transform.position - transform.position) : transform.rotation;

                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 85.0f * Time.fixedDeltaTime);
            }
            else if (!_isAggro && _returnAnim)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _defaultRotation, 35.0f * Time.fixedDeltaTime);
            }
        }

        private void DistanceCheckerLoop()
        {
            if (_active)
            {
                if (_player != null && !_isChangingState)
                {
                    if (Vector3.Distance(transform.position, _player.transform.position) < spawnActivationDistance && !_isSpawned)
                    {
                        UpdateFOVLight(true, false);
                        StopAllCoroutines();
                        StartCoroutine(ChangeState(true));
                    }
                    else if (Vector3.Distance(transform.position, _player.transform.position) > spawnActivationDistance + 5.0f &&
                             _isSpawned)
                    {
                        UpdateFOVLight(false, false);
                        StopAllCoroutines();
                        if (gameObject.activeSelf)
                            StartCoroutine(ChangeState(false));
                    }
                }
            }
        }

        private void UpdateFOVLight(bool on, bool aggro)
        {
            if (fieldOfViewLight != null)
            {
                if (on && !aggro)
                {
                    fieldOfViewLight.color = normalConeColor;
                    _lightOn = true;
                }
                else if (on && aggro)
                {
                    fieldOfViewLight.color = aggroConeColor;
                    _lightOn = true;
                }
                else
                    _lightOn = false;
            }
        }

        public void ResetBig()
        {
            if (_animator != null)
            {
                StopAllCoroutines();
                _isAggro = false;
                _returnAnim = false;
                _playerSpotted = false;
                _timerRunning = false;
                _isChangingState = false;
                //_playerMovement.SetActive(true);
                _playerMovement.Frozen(false);
                _animator.ResetTrigger("Appearing");
                _playerAnimator?.ResetTrigger("BigAttack");
                _animator.ResetTrigger("Attack");
                _animator.SetBool("Patrolling", false);
                _animator.SetTrigger("Reset");
                transform.rotation = _defaultRotation;
                StartCoroutine(ChangeState(false));
                UpdateFOVLight(false, false);
            }
        }

        private IEnumerator ChangeState(bool isActive)
        {
            _isChangingState = true;

            if (isActive)
            {
                _animator.SetTrigger("Appearing");
                enemySoundManager?.GettingUp();
            }
            else
            {
                _animator.SetTrigger("Disappearing");
                enemySoundManager?.PushedDown();
            }

            yield return new WaitForSeconds(changeStateTime);

            if (isActive && !_isAggro)
            {
                _animator.SetBool("Patrolling", true);
                _isSpawned = true;
            }
            else if (!_isAggro)
            {
                _animator.SetBool("Patrolling", false);
                _isSpawned = false;
            }

            _isChangingState = false;
        }

        private IEnumerator PlayerDied()
        {
            _playerMovement.Frozen(true);
            //_playerMovement.SetActive(false);
            _active = false;

            _animator.SetTrigger("Attack");
            _playerAnimator.SetTrigger("BigAttack");
            enemySoundManager?.AttackPlayer();

            yield return new WaitForSeconds(animAttackTime/1.5f);

            playerDiedEvent?.Raise();

            _active = true;
            _returnAnim = false;
        }

        private IEnumerator Aggro()
        {
            _isAggro = true;

            if (!_isSpawned)
                _isSpawned = true;

            _animator.SetBool("Patrolling", false);
            _animator.ResetTrigger("Appearing");
            _animator.SetTrigger("Spotted");
            enemySoundManager?.Spotted();

            yield return new WaitForSeconds(aggroTime);

            Vector3 dir = _player.transform.position - visionGameObject.transform.position;
            RaycastHit hit;

            if (Physics.Raycast(visionGameObject.transform.position, dir, out hit, viewDistance, _layerMask))
            {
                if (hit.collider.tag == _player.tag)
                    StartCoroutine(PlayerDied());
                else
                    CancelAggro();
            }
            else
                CancelAggro();
        }

        private void CancelAggro()
        {
            UpdateFOVLight(true, false);
            _isAggro = false;
            _active = true;
            _returnAnim = true;
            _animator.SetBool("Patrolling", true);
            _animator.ResetTrigger("Spotted");
            StopAllCoroutines();
            StartCoroutine(ReturnTimer());
        }

        private IEnumerator ReturnTimer()
        {
            yield return new WaitForSeconds(1.5f);

            _returnAnim = false;
        }


//#if UNITY_EDITOR
//        private void OnDrawGizmos()
//        {
//            if (drawGizmos && Application.isEditor && visionGameObject != null)
//            {
//                Gizmos.color = Color.red;

//                Gizmos.DrawLine(visionGameObject.transform.position + visionGameObject.transform.up, visionGameObject.transform.forward * viewDistance + (visionGameObject.transform.position + visionGameObject.transform.up));
//            }
//        }
//#endif
    }
}