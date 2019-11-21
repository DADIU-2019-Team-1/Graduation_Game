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
        private Animator _playerAnimator;
        private EnemySoundManager _enemySoundManager;
        private Animator _animator;
        public Light fieldOfViewLight;
        public VoidEvent playerDiedEvent;
        public GameObject visionGameObject;

        // Private:
        private bool _active, _isAggro, _isSpawned, _isRotating, _turnLeft, _updateRotation, _playerSpotted, _lightOn, _timerRunning;
        private int _layerMask, _currentSpawnPoint = 0;
        private Vector3 _lookRangeToVector, _lookRangeFromVector, _spawnedPos, _unSpawnedPos;
        private Quaternion _lookRotation;

        private bool _appear, _disappear; // TODO: Remove this later (they are used as placeholders for animation atm.)

        // Public:
        public bool drawGizmos = true;
        public float spawnActivationDistance = 25.0f, fieldOfView = 65.0f, viewDistance = 20.0f, changeStateTime = 3.0f,
            rotateDegreesPerSecond = 5.0f, rotateWaitTime = 0.0f, lookRangeTo = 230f, lookRangeFrom = 130f, aggroTime = 2.0f;
        public Color normalConeColor = Color.yellow, aggroConeColor = Color.red;
        public float animAttackTime = 3.0f;


        private void Awake()
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                _player = GameObject.FindGameObjectWithTag("Player");
                _playerAnimator = _player.GetComponent<Animator>();
            }

            if (GetComponent<Animator>() != null)
            {
                _animator = GetComponent<Animator>();
                //_animator.SetTrigger("Patrolling");
            }
            else
            {
                _active = false;
                Debug.LogError("Big Sis Error: No animator on " + gameObject.name);
            }

            if (GetComponent<EnemySoundManager>() != null)
                _enemySoundManager = gameObject.GetComponent<EnemySoundManager>();

            _layerMask = ~LayerMask.GetMask("Enemies");

            _lookRangeToVector = new Vector3(0, lookRangeTo, 0);
            _lookRangeFromVector = new Vector3(0, lookRangeFrom, 0);

            if (fieldOfViewLight != null)
            {
                fieldOfViewLight.range = viewDistance;
                fieldOfViewLight.spotAngle = fieldOfView;
            }

            _unSpawnedPos = visionGameObject.transform.position;
            _spawnedPos = visionGameObject.transform.position + (visionGameObject.transform.up * 8);

            if (visionGameObject != null)
                _active = true;
            else
                Debug.LogError("Big Enemy Error: Vision gameobject missing, please attach one!");
        }

        private void FixedUpdate()
        {
            if (_active)
            {
                if (_isSpawned)
                {
                    if (!_lightOn)
                        UpdateFOVLight(true, false);

                    // Rotation code:   TODO: uncomment if neeeded again
                    //if (_isRotating)
                    //{
                    //    if (_updateRotation)
                    //    {
                    //        _updateRotation = false;

                    //        if (_turnLeft)
                    //            _lookRotation = Quaternion.Euler(_lookRangeToVector) != Quaternion.identity ? Quaternion.Euler(_lookRangeToVector) : transform.rotation;
                    //        else
                    //            _lookRotation = Quaternion.Euler(_lookRangeFromVector) != Quaternion.identity ? Quaternion.Euler(_lookRangeFromVector) : transform.rotation;
                    //    }

                    //    transform.rotation = Quaternion.RotateTowards(transform.rotation, _lookRotation, rotateDegreesPerSecond * Time.fixedDeltaTime);

                    //    if (transform.rotation.eulerAngles.y >= lookRangeTo - 3.0f && _turnLeft)
                    //    {
                    //        if (rotateWaitTime > 0 && !_timerRunning)
                    //        {
                    //            _turnLeft = false;
                    //            StartCoroutine(WaitTimer());
                    //        }
                    //        else if (!_timerRunning)
                    //        {
                    //            _turnLeft = false;
                    //            _updateRotation = true;
                    //        }
                    //    }
                    //    else if (transform.rotation.eulerAngles.y <= lookRangeFrom + 3.0f && !_turnLeft)
                    //    {
                    //        if (rotateWaitTime > 0 && !_timerRunning)
                    //        {
                    //            _turnLeft = true;
                    //            _updateRotation = true;
                    //            StartCoroutine(WaitTimer());
                    //        }
                    //        else if (!_timerRunning)
                    //        {
                    //            _turnLeft = true;
                    //            _updateRotation = true;
                    //        }
                    //    }
                    //}

                    if (!_playerSpotted)
                    {
                        if (_player != null)
                        {
                            Vector3 dir = _player.transform.position - visionGameObject.transform.position;
                            float enemyToPlayerAngle = Vector3.Angle(visionGameObject.transform.forward, dir);

                            if (enemyToPlayerAngle < fieldOfView / 2)
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

                if (_player != null)
                {
                    if (Vector3.Distance(transform.position, _player.transform.position) < spawnActivationDistance && !_isSpawned)
                    {
                        _appear = true;
                        _disappear = false;
                        UpdateFOVLight(true, false);
                        StopCoroutine(ChangeState(false));
                        StartCoroutine(ChangeState(true));
                    }
                    else if (Vector3.Distance(transform.position, _player.transform.position) > spawnActivationDistance &&
                             _isSpawned)
                    {
                        _disappear = true;
                        _appear = false;
                        UpdateFOVLight(false, false);
                        StopCoroutine(ChangeState(true));
                        StartCoroutine(ChangeState(false));
                    }
                }

                //if (visionGameObject != null) // TODO: Enable again if needed
                //{
                //    if (_appear && !_disappear)
                //    {
                //        visionGameObject.transform.position = Vector3.Lerp(visionGameObject.transform.position, _spawnedPos, Time.fixedDeltaTime);

                //        if (transform.position == _spawnedPos)
                //            _appear = false;
                //    }
                //    else if (_disappear && !_appear)
                //    {
                //        visionGameObject.transform.position = Vector3.Lerp(visionGameObject.transform.position, _unSpawnedPos, Time.fixedDeltaTime);

                //        if (visionGameObject.transform.position == _unSpawnedPos)
                //            _disappear = false;
                //    }
                //}
            }

            if (_isAggro && !_active)
            {
                Vector3 dir = _player.transform.position - transform.position;

                Quaternion rot = Quaternion.LookRotation(_player.transform.position - transform.position) != Quaternion.identity ? Quaternion.LookRotation(_player.transform.position - transform.position) : transform.rotation;

                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 85.0f * Time.fixedDeltaTime);
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
                _disappear = true;
                _appear = false;
                _isAggro = false;
                _isSpawned = false;
                _playerSpotted = false;
                _timerRunning = false;
                _animator.ResetTrigger("Appearing");
                _playerAnimator?.ResetTrigger("BigAttack");
                _animator.ResetTrigger("Attack");
                _animator.SetBool("Patrolling", false);
                _animator.SetTrigger("Disappearing");
                UpdateFOVLight(false, false);
            }
        }

        private IEnumerator ChangeState(bool isActive)
        {
            if (isActive)
            {
                _animator.SetTrigger("Appearing");
                _enemySoundManager?.gettingUp();
            }
            else
            {
                _animator.SetTrigger("Disappearing");
                _enemySoundManager?.pushedDown();
            }

            yield return new WaitForSeconds(changeStateTime);

            if (isActive && !_isAggro)
            {
                _animator.ResetTrigger("Appearing");
                _animator.SetBool("Patrolling", true);
                _isSpawned = true;
            }
            else if (!_isAggro)
            {
                _animator.ResetTrigger("Disappearing");
                _animator.SetBool("Patrolling", false);
                _isSpawned = false;
            }
        }

        private IEnumerator PlayerDied()
        {
            _player.GetComponent<Movement>().Frozen(true);
            _active = false;

            _animator.SetTrigger("Attack");
            _playerAnimator.SetTrigger("BigAttack");
            _enemySoundManager?.attackPlayer();

            yield return new WaitForSeconds(animAttackTime);

            _playerAnimator?.ResetTrigger("BigAttack");
            _animator?.ResetTrigger("Attack");
            _active = true;
            playerDiedEvent?.Raise();
        }

        private IEnumerator Aggro()
        {
            _isAggro = true;

            if (!_isSpawned)
                _isSpawned = true;

            _animator.SetBool("Patrolling", false);
            _animator.ResetTrigger("Appearing");
            _animator.SetTrigger("Spotted");
            _enemySoundManager?.spotted();

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
            _animator.SetBool("Patrolling", true);
            _animator.ResetTrigger("Spotted");
        }

        private IEnumerator WaitTimer()
        {
            _timerRunning = true;
            _isRotating = false;
            yield return new WaitForSeconds(rotateWaitTime);

            _isRotating = true;
            _updateRotation = true;
            _timerRunning = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (drawGizmos && Application.isEditor && visionGameObject != null)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawLine(visionGameObject.transform.position + visionGameObject.transform.up, visionGameObject.transform.forward * viewDistance + (visionGameObject.transform.position + visionGameObject.transform.up));
            }
        }
#endif
    }
}