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
        private Vector3[] _lookRangeTo, _lookRangeFrom;
        private Quaternion _lookRotation;

        // Public:
        [HideInInspector] public List<GameObject> spawnPoints;
        public bool drawGizmos = true;
        public float spawnActivationDistance = 25.0f, fieldOfView = 90.0f, viewDistance = 20.0f, headRotateSpeed = 1.0f, rotateWaitTime = 0.0f;
        public Color normalConeColor = Color.yellow, aggroConeColor = Color.red;


        private void Awake()
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
                _player = GameObject.FindGameObjectWithTag("Player");

            if (GetComponent<Animator>() != null)
                _animator = GetComponent<Animator>();

            _layerMask = ~LayerMask.GetMask("Enemies");

            if (spawnPoints != null)
            {
                _lookRangeFrom = new Vector3[spawnPoints.Count];
                _lookRangeTo = new Vector3[spawnPoints.Count];
            }

            for (int i = 0; i < spawnPoints.Count; i++)
            {
                if (spawnPoints[i].GetComponent<WayPoint>() != null)
                {
                    _lookRangeTo[i] = new Vector3(0, spawnPoints[i].GetComponent<Big_SpawnPoint>().lookRangeTo, 0);
                    _lookRangeFrom[i] = new Vector3(0, spawnPoints[i].GetComponent<Big_SpawnPoint>().lookRangeFrom, 0);
                }
            }
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
                        _lookRotation = Quaternion.Euler(_lookRangeTo[_currentSpawnPoint]) != Quaternion.identity ? Quaternion.Euler(_lookRangeTo[_currentSpawnPoint]) : transform.rotation;
                    else
                        _lookRotation = Quaternion.Euler(_lookRangeFrom[_currentSpawnPoint]) != Quaternion.identity ? Quaternion.Euler(_lookRangeFrom[_currentSpawnPoint]) : transform.rotation;

                    transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, headRotateSpeed);

                    if (transform.rotation.y >= _lookRangeTo[_currentSpawnPoint].y || transform.rotation.y <= _lookRangeFrom[_currentSpawnPoint].y)
                    {
                        _turnLeft = !_turnLeft;
                    }
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
        #region SpawnPoint System
        public void AddSpawnPoint()
        {
            if (Application.isEditor)
            {
                GameObject tempWayPointObj;

                if (!GameObject.Find("BigSpawnpoints"))
                    new GameObject("BigSpawnpoints");

                if (!GameObject.Find(gameObject.name + "_Spawnpoints"))
                {
                    _parentSpawnPoint = new GameObject(gameObject.name + "_Spawnpoints");

                    _parentSpawnPoint.AddComponent<Big_SpawnPoint>();
                    _parentSpawnPoint.GetComponent<Big_SpawnPoint>().isParent = true;
                    _parentSpawnPoint.transform.parent =
                        GameObject.Find("BigSpawnpoints").transform;
                }
                else
                {
                    _parentSpawnPoint = GameObject.Find(gameObject.name + "_Spawnpoints");
                }

                if (spawnPoints == null)
                {
                    spawnPoints = new List<GameObject>();
                }

                tempWayPointObj = new GameObject("SpawnPoint" + (spawnPoints.Count + 1));
                tempWayPointObj.AddComponent<Big_SpawnPoint>();
                Big_SpawnPoint tempWayPointScript = tempWayPointObj.GetComponent<Big_SpawnPoint>();
                tempWayPointScript.parentObject = _parentSpawnPoint;

                tempWayPointObj.transform.position = gameObject.transform.position;
                tempWayPointObj.transform.parent = _parentSpawnPoint.transform;
                spawnPoints.Add(tempWayPointObj);
            }
        }

        public void RemoveSpawnPoint()
        {
            if (Application.isEditor)
            {
                if (spawnPoints != null)
                    if (spawnPoints.Count > 0)
                    {
                        DestroyImmediate(spawnPoints[spawnPoints.Count - 1].gameObject);
                    }
            }
        }

        private void OnDrawGizmos()
        {
            if (drawGizmos && Application.isEditor)
                if (spawnPoints != null)
                {
                    for (int i = 0; i < spawnPoints.Count; i++)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireSphere(spawnPoints[i].transform.position, 0.3f);
                        Handles.Label(spawnPoints[i].transform.position + (Vector3.up * 0.6f), (i + 1).ToString());

                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(transform.position + transform.up, transform.forward * viewDistance + (transform.position + transform.up));
                    }
                }
        }
        #endregion
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

            if (script != null)
            {
                if (script.spawnPoints.Count == 0)
                {
                    _style.normal.textColor = Color.red;
                }
                else
                {
                    _style.normal.textColor = Color.green;
                }

                EditorGUILayout.LabelField(script.spawnPoints.Count.ToString() + " spawnpoints", _style);
            }
            else
            {
                _style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("0 spawnpoints", _style);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add SpawnPoint"))
            {
                script.AddSpawnPoint();
            }

            if (GUILayout.Button("Remove SpawnPoint"))
            {
                script.RemoveSpawnPoint();
            }

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