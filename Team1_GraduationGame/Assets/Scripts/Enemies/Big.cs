using System.Collections;
using System.Collections.Generic;
using Team1_GraduationGame.Enemies;
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

        // Private:
        private GameObject _parentSpawnPoint;
        private bool _isSpawned, _isRotating, _turnLeft;
        private int _layerMask, _currentSpawnPoint = 0;
        private Vector3 _lookRangeToVector, _lookRangeFromVector;
        private Quaternion _lookRotation;

        // Public:
        [HideInInspector] public List<GameObject> spawnPoints;
        public bool drawGizmos = true;
        public float spawnActivationDistance = 25.0f, fieldOfView = 90.0f, viewDistance = 20.0f, 
            headRotateSpeed = 0.05f, rotateWaitTime = 0.0f, lookRangeTo = 230f, lookRangeFrom = 130f;
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

        }

        private void Start()
        {
            _isRotating = true;
        }

        private void FixedUpdate()
        {
            if (_isRotating)    // TODO make this work
            {
                if (_currentSpawnPoint >= 0 && _currentSpawnPoint < spawnPoints.Count)
                {
                    if (_turnLeft)
                        _lookRotation = Quaternion.Euler(_lookRangeToVector) != Quaternion.identity ? Quaternion.Euler(_lookRangeToVector) : transform.rotation;
                    else
                        _lookRotation = Quaternion.Euler(_lookRangeFromVector) != Quaternion.identity ? Quaternion.Euler(_lookRangeFromVector) : transform.rotation;

                    transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, headRotateSpeed * Time.fixedDeltaTime);
                    //Debug.Log(_lookRotation);

                #if UNITY_EDITOR
                    Vector3 fwd = _lookRotation * Vector3.forward;
                    Debug.DrawRay(transform.position, fwd, Color.magenta, 0f, true);
                #endif

                    //if (transform.rotation.y <= _lookRangeTo[_currentSpawnPoint].y || transform.rotation.y >= _lookRangeFrom[_currentSpawnPoint].y)
                    //{
                    //    Debug.Log("DIRECTION SWITCH");
                    //    _turnLeft = !_turnLeft;
                    //}
                }
            }

            if (_player != null && spawnPoints != null)
            {
                if (Vector3.Distance(transform.position, _player.transform.position) < spawnActivationDistance)
                {

                }
            }


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
}