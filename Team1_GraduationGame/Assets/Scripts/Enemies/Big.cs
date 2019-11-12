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
        private bool _active, _isSpawned, _isRotating, _turnLeft, _updateRotation, _playerSpotted, _lightOn, _timerRunning;
        private int _layerMask, _currentSpawnPoint = 0;
        private Vector3 _lookRangeToVector, _lookRangeFromVector;
        private Quaternion _lookRotation;

        // Public:
        [HideInInspector] public List<GameObject> spawnPoints;
        public bool drawGizmos = true;
        public float spawnActivationDistance = 25.0f, fieldOfView = 65.0f, viewDistance = 20.0f, 
            rotateDegreesPerSecond = 5.0f, rotateWaitTime = 0.0f, lookRangeTo = 230f, lookRangeFrom = 130f;
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

            _active = true;
        }

        private void Start()
        {
            // TODO remove this after debugging:
            _isRotating = true;
            _isSpawned = true;
            _updateRotation = true;
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
                                StartCoroutine(WaitTimer());
                            }
                            else
                            {
                                _turnLeft = false;
                                _updateRotation = true;
                            }
                        }
                        else if (transform.rotation.eulerAngles.y <= lookRangeFrom + 3.0f && !_turnLeft)
                        {
                            if (rotateWaitTime > 0 && !_timerRunning)
                            {
                                StartCoroutine(WaitTimer());
                            }
                            else
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
                                        Debug.Log("PLAYER SPOTTED");
                                        _active = false;

                                        transform.LookAt(dir);
                                        _player.GetComponent<Movement>().Frozen(true);

                                        StartCoroutine(PlayerDied());

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

                if (_player != null && spawnPoints != null)
                {
                    if (Vector3.Distance(transform.position, _player.transform.position) < spawnActivationDistance && !_isSpawned)
                    {
                        Debug.Log("Player CLOSE");
                        _isRotating = true;
                        _isSpawned = true;
                        //_updateRotation = true;
                    }
                    else if (Vector3.Distance(transform.position, _player.transform.position) > spawnActivationDistance &&
                             _isSpawned)
                    {
                        // _isRotating = false;
                        //_isSpawned = false;
                        //_updateRotation = false;
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
                {
                    _lightOn = false;
                }
            }
        }

        private IEnumerator PlayerDied()
        {
            yield return new WaitForSeconds(5.0f); // TODO Specify amount of time animation takes instead
            if (playerDiedEvent != null)
                playerDiedEvent.Raise();
        }

        private IEnumerator WaitTimer()
        {
            _timerRunning = true;
            _isRotating = false;
            yield return new WaitForSeconds(rotateWaitTime);

            if (_turnLeft)
                _turnLeft = false;
            else
                _turnLeft = true;

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
        private GUIStyle _style = new GUIStyle();
        private GameObject _parentWayPoint;
        private bool _runOnce;

        public override void OnInspectorGUI()
        {
            if (!_runOnce)
            {
                _style.fontStyle = FontStyle.Bold;
                _style.alignment = TextAnchor.MiddleCenter;
                _style.fontSize = 14;
                _runOnce = true;
            }

            DrawDefaultInspector(); // for other non-HideInInspector fields

            var script = target as Big;

            DrawUILine();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
            }
        }

        #region DrawUILine function
        public static void DrawUILine()
        {
            Color color = new Color(1, 1, 1, 0.3f);
            int thickness = 1;
            int padding = 8;

            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
        #endregion
    }
    #endregion
#endif
}