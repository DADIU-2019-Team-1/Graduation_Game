using UnityEngine.AI;
using UnityEngine.UIElements;

namespace Team1_GraduationGame.Enemies
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;

#endif

    [RequireComponent(typeof(SphereCollider), typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        // Scriptable Object References:
        public BaseEnemy thisEnemy;

        // Public variables:
        [Tooltip("How close the enemy should be to the waypoint for it to switch to the next")]
        [Range(0.0f, 10.0f)] public float wayPointReachRange = 1.0f, aggroTime = 4.0f, attackTime = 2.0f;
        public bool drawGizmos = true;
        [Tooltip("If > 0 the enemy will wait before going to next waypoint")] public float waitTime = 0.0f;

        [HideInInspector] public List<Transform> wayPoints;
        [HideInInspector] public GameObject parentWayPoint;

        // Private variables:
        private bool _active, _useTimer, _timerRunning, _destinationSet, _isRotating, _isAggro, _isHugging;
        private NavMeshAgent _navMeshAgent;
        private Vector3 _lastSighting, _fieldOfView;
        private GameObject _player;
        private SphereCollider _thisCollider;
        private int currentWayPoint = 0;

        void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");

            if (waitTime > 0)
            {
                _useTimer = true;
            }

            if (_player != null && thisEnemy != null)
            {
                _active = true;
            }
            else
                Debug.LogError("Enemy Error: Player not found or scriptable object not attached!");


            if (_active)
            {
                _thisCollider = gameObject.GetComponent<SphereCollider>();
                _fieldOfView = new Vector3(0, 0, thisEnemy.viewAngleRange);

                if (_thisCollider != null)
                {
                    _thisCollider.isTrigger = true;
                    _thisCollider.radius = thisEnemy.viewDistance;
                }


                if (GetComponent<NavMeshAgent>() != null)
                {
                    _navMeshAgent = GetComponent<NavMeshAgent>();
                    _navMeshAgent.speed = thisEnemy.speed;
                    _navMeshAgent.angularSpeed = thisEnemy.turnSpeed;
                    _navMeshAgent.updateRotation = true;
                }
                else
                {
                    _active = false;
                    Debug.LogError("Enemy Error: No Nav Mesh Agent script attached!");
                }

                if (wayPoints == null)
                {
                    wayPoints = new List<Transform>();
                }

            }
        }

        private void FixedUpdate()
        {
            if (_active)
            {
                if (!_isAggro && wayPoints != null && wayPoints.Count != 0)
                {

                    if (Vector3.Distance(transform.position, wayPoints[currentWayPoint].position) < wayPointReachRange)
                    {
                        _isRotating = false;

                        if (!_useTimer)
                        {
                            currentWayPoint = (currentWayPoint + 1) % wayPoints.Count;
                            _destinationSet = false;
                        }
                        else if (_useTimer && !_timerRunning)
                        {
                            StartCoroutine(WaitTimer());
                            _timerRunning = true;
                        }

                    }

                    if (!_destinationSet)
                    {
                        _navMeshAgent.SetDestination(wayPoints[currentWayPoint].position);
                        _destinationSet = true;
                        _isRotating = true;
                    }

                }

                if (_isAggro)
                {
                    _navMeshAgent.SetDestination(_lastSighting);
                    _isRotating = true;

                    if (Vector3.Distance(transform.position, _player.transform.position) <
                        wayPointReachRange)
                    {
                        if (!_isHugging)
                            StartCoroutine(EnemyHug());
                    }
                    else
                    if (_isHugging)
                        StopCoroutine(EnemyHug());
                }

                if (_isRotating)
                {
                    Quaternion lookRotation = _navMeshAgent.velocity.normalized != Vector3.zero ? Quaternion.LookRotation(_navMeshAgent.velocity.normalized) : transform.rotation; // Avoid LookRotation zero-errors 
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, thisEnemy.turnSpeed * Time.fixedDeltaTime);
                }
            }
            
        }

        private void OnTriggerStay(Collider col)
        {
            if (col.tag == _player.tag)
            {
                Vector3 dir = _player.transform.position - transform.position;
                float enemyToPlayerAngle = Vector3.Angle(transform.forward, dir);

                if (enemyToPlayerAngle < thisEnemy.viewAngleRange / 2)
                {
                    RaycastHit hit;

                    if (Physics.Raycast(transform.position + transform.up, dir, out hit, thisEnemy.viewDistance))
                        if (hit.collider.gameObject == _player)
                        {
                            _lastSighting = _player.transform.position;
                            if (!_isAggro)
                                StartCoroutine(EnemyAggro());
                        }
                } else if (_isAggro)
                {
                    _lastSighting = _player.transform.position;
                }

            }
        }

        private IEnumerator WaitTimer()
        {
            yield return new WaitForSeconds(waitTime);
            currentWayPoint = (currentWayPoint + 1) % wayPoints.Count;
            _destinationSet = false;
            _timerRunning = false;
        }

        private IEnumerator EnemyAggro()
        {
            _isAggro = true;
            yield return new WaitForSeconds(aggroTime);
            _isAggro = false;

            yield return new WaitForSeconds(0.3f);
            if (!_isAggro)
                _destinationSet = false;
        }

        private IEnumerator EnemyHug()
        {
            _isHugging = true;
            yield return new WaitForSeconds(attackTime);
            _isHugging = false;
            _isAggro = false;
            Debug.Log("THE PLAYER DIED");
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (drawGizmos)
                if (wayPoints != null)
                {
                    for (int i = 0; i < wayPoints.Count; i++)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireSphere(wayPoints[i].position, 0.2f);

                        Gizmos.color = Color.yellow;
                        if (i + 1 < wayPoints.Count)
                        {
                            Gizmos.DrawLine(wayPoints[i].position, wayPoints[i + 1].position);

                        }
                        else if (i == wayPoints.Count - 1 && wayPoints.Count > 1)
                        {
                            Gizmos.DrawLine(wayPoints[wayPoints.Count - 1].position, wayPoints[0].position);
                        }

                        if (thisEnemy != null)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(transform.position, transform.forward * thisEnemy.viewDistance + transform.position);
                            //Gizmos.DrawLine(transform.position, (transform.forward) * thisEnemy.viewDistance + transform.position);
                            //Debug.Log(transform.forward + " / " + (transform.forward + _fieldOfView.normalized / 2) * thisEnemy.viewDistance + transform.position);
                        }
                    }
                }

        }

#endif

    }

    #region Custom Inspector
#if UNITY_EDITOR
    [CustomEditor(typeof(Enemy))]
    public class Enemy_Inspector : Editor
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

            var script = target as Enemy;

            DrawUILine(false);

            if (script.wayPoints != null)
            {
                if (script.wayPoints.Count == 0)
                {
                    _style.normal.textColor = Color.red;
                }
                else
                {
                    _style.normal.textColor = Color.green;
                }

                EditorGUILayout.LabelField(script.wayPoints.Count.ToString() + " waypoints active", _style);
            }
            else
            {
                _style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("0 waypoints active", _style);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add WayPoint"))
            {
                GameObject tempWayPointObj;

                if (!GameObject.Find(script.gameObject.name + "_Waypoints"))
                {
                    script.parentWayPoint = new GameObject(script.gameObject.name + "_Waypoints");
                    _parentWayPoint = script.parentWayPoint;
                }
                else
                {
                    _parentWayPoint = GameObject.Find(script.gameObject.name + "_Waypoints");
                }

                if (script.wayPoints == null)
                {
                    script.wayPoints = new List<Transform>();
                }

                tempWayPointObj = new GameObject("WayPoint" + script.wayPoints.Count);

                tempWayPointObj.transform.position = tempWayPointObj.transform.up;
                tempWayPointObj.transform.parent = _parentWayPoint.transform;
                script.wayPoints.Add(tempWayPointObj.transform);
            }

            if (GUILayout.Button("Remove WayPoint"))
            {
                if (script.wayPoints != null)
                    if (script.wayPoints.Count > 0)
                    {
                        DestroyImmediate(script.wayPoints[script.wayPoints.Count - 1].gameObject);
                        script.wayPoints.RemoveAt(script.wayPoints.Count - 1);

                        if (script.wayPoints.Count < 1 && script.parentWayPoint != null)
                        {
                            DestroyImmediate(script.parentWayPoint);
                        }
                    }
            }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
            }
        }

        #region DrawUILine function
        public static void DrawUILine(bool start)
        {
            Color color = new Color(1, 1, 1, 0.3f);
            int thickness = 1;
            if (start)
                thickness = 7;
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
#endif
    #endregion
}
