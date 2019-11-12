using System.Collections;
using System.Collections.Generic;
using Team1_GraduationGame.Enemies;
using Team1_GraduationGame.Events;
using UnityEditor;
using UnityEngine;

namespace Team1_GraduationGame.Enemies
{
    public class Big : MonoBehaviour
    {
        //References:
        private GameObject _player;
        private Animator _animator;
        public Light fieldOfViewLight;
        public VoidEvent playerDiedEvent;

        // Private:
        private bool _active, _isAggro, _isSpawned, _isRotating, _turnLeft, _updateRotation, _playerSpotted, _lightOn, _timerRunning;
        private int _layerMask, _currentSpawnPoint = 0;
        private Vector3 _lookRangeToVector, _lookRangeFromVector, _spawnedPos, _unSpawnedPos;
        private Quaternion _lookRotation;
        private bool _appear, _disappear; // TODO: Remove this later (they are used as placeholders for animation atm.)

        // Public:
        public bool drawGizmos = true;
        public float spawnActivationDistance = 25.0f, fieldOfView = 65.0f, viewDistance = 20.0f, 
            rotateDegreesPerSecond = 5.0f, rotateWaitTime = 0.0f, lookRangeTo = 230f, lookRangeFrom = 130f, aggroTime = 2.0f;
        public Color normalConeColor = Color.yellow, aggroConeColor = Color.red;


        private void Awake()
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
                _player = GameObject.FindGameObjectWithTag("Player");

            if (GetComponent<Animator>() != null)
                _animator = GetComponent<Animator>();

            _layerMask = ~LayerMask.GetMask("Enemies");

            _lookRangeToVector = new Vector3(0, lookRangeTo, 0);
            _lookRangeFromVector = new Vector3(0, lookRangeFrom, 0);

            if (fieldOfViewLight != null)
            {
                fieldOfViewLight.range = viewDistance;
                fieldOfViewLight.spotAngle = fieldOfView;
            }

            _unSpawnedPos = transform.position;
            _spawnedPos = transform.position + (transform.up * 6);

            _active = true;
        }

        private void FixedUpdate()
        {
            if (_active)
            {
                if (_isSpawned)
                {
                    if (!_lightOn)
                        UpdateFOVLight(true, false);

                    if (_isRotating)
                    {
                        if (_updateRotation)
                        {
                            _updateRotation = false;

                            if (_turnLeft)
                                _lookRotation = Quaternion.Euler(_lookRangeToVector) != Quaternion.identity ? Quaternion.Euler(_lookRangeToVector) : transform.rotation;
                            else
                                _lookRotation = Quaternion.Euler(_lookRangeFromVector) != Quaternion.identity ? Quaternion.Euler(_lookRangeFromVector) : transform.rotation;
                        }

                        transform.rotation = Quaternion.RotateTowards(transform.rotation, _lookRotation, rotateDegreesPerSecond * Time.fixedDeltaTime);

                        if (transform.rotation.eulerAngles.y >= lookRangeTo - 3.0f && _turnLeft)
                        {
                            if (rotateWaitTime > 0 && !_timerRunning)
                            {
                                _turnLeft = false;
                                StartCoroutine(WaitTimer());
                            }
                            else if (!_timerRunning)
                            {
                                _turnLeft = false;
                                _updateRotation = true;
                            }
                        }
                        else if (transform.rotation.eulerAngles.y <= lookRangeFrom + 3.0f && !_turnLeft)
                        {
                            if (rotateWaitTime > 0 && !_timerRunning)
                            {
                                _turnLeft = true;
                                _updateRotation = true;
                                StartCoroutine(WaitTimer());
                            }
                            else if (!_timerRunning)
                            {
                                _turnLeft = true;
                                _updateRotation = true;
                            }
                        }
                    }

                    if (!_playerSpotted)
                    {
                        if (_player != null)
                        {
                            Vector3 dir = _player.transform.position - transform.position;
                            float enemyToPlayerAngle = Vector3.Angle(transform.forward, dir);

                            if (enemyToPlayerAngle < fieldOfView / 2)
                            {
                                RaycastHit hit;

                                if (Physics.Raycast(transform.position + transform.up, dir, out hit, viewDistance, _layerMask))
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

                if (_appear && !_disappear)
                {
                    transform.position = Vector3.Lerp(transform.position, _spawnedPos, Time.fixedDeltaTime);

                    if (transform.position == _spawnedPos)
                        _appear = false;
                }
                else if (_disappear && !_appear)
                {
                    transform.position = Vector3.Lerp(transform.position, _unSpawnedPos, Time.fixedDeltaTime);

                    if (transform.position == _unSpawnedPos)
                        _disappear = false;
                }
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
                {
                    _lightOn = false;
                }
            }
        }

        private IEnumerator ChangeState(bool isActive)
        {
            yield return new WaitForSeconds(3.0f);

            if (isActive)
            {
                _isRotating = true;
                _isSpawned = true;
                _updateRotation = true;
            }
            else
            {
                _isRotating = false;
                _isSpawned = false;
                _updateRotation = false;
            }

        }

        private IEnumerator PlayerDied()
        {
            _player.GetComponent<Movement>().Frozen(true);

            yield return new WaitForSeconds(2.0f); // TODO Specify amount of time animation takes instead
            Debug.Log("Player died from Big");
            if (playerDiedEvent != null)
                playerDiedEvent.Raise();
        }

        private IEnumerator Aggro()
        {
            _isAggro = true;

            yield return new WaitForSeconds(aggroTime);

            Vector3 dir = _player.transform.position - transform.position;
            RaycastHit hit;

            if (Physics.Raycast(transform.position + transform.up, dir, out hit, viewDistance, _layerMask))
            {
                if (hit.collider.tag == _player.tag)
                {
                    StartCoroutine(PlayerDied());
                }
                else
                {
                    UpdateFOVLight(true, false);
                    _isAggro = false;
                    _active = true;
                }
            }
            else
            {
                UpdateFOVLight(true, false);
                _isAggro = false;
                _active = true;
            }
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
            if (drawGizmos && Application.isEditor)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawLine(transform.position + transform.up, transform.forward * viewDistance + (transform.position + transform.up));
            }
        }
        #endif
    }

#if UNITY_EDITOR
    #region Custom Editor
    [CustomEditor(typeof(Big))]
    public class Big_Editor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // for other non-HideInInspector fields

            var script = target as Big;

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
            }
        }
    }
    #endregion
#endif
}