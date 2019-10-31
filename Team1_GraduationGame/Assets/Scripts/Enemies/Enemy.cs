namespace Team1_GraduationGame.Enemies
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Team1_GraduationGame.Events;
    using UnityEngine.AI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [RequireComponent(typeof(SphereCollider), typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        #region Variables

        // References:
        public BaseEnemy thisEnemy;
        public VoidEvent playerDiedEvent;
        public Light viewConeLight;

        // Public variables:
        public float wayPointReachRange = 1.0f, aggroTime = 5.0f, attackTime = 0.25f;
        public bool drawGizmos = true, useWaitTime, rotateAtWaypoints, loopWaypointRoutine = true;
        public Color normalConeColor = Color.yellow, aggroConeColor = Color.red;
        [HideInInspector] public bool useGlobalWaitTime = true;
        [HideInInspector] public float waitTime = 0.0f;
        [HideInInspector] public List<GameObject> wayPoints;
        [HideInInspector] public GameObject parentWayPoint;

        // Private variables:
        private bool _active, _timerRunning, _destinationSet, _isRotating, _isAggro, _isHugging, _rotatingAtWp;
        private NavMeshAgent _navMeshAgent;
        private NavMeshPath _path;
        private Vector3 _lastSighting;
        private Vector3[] _wayPointRotations;
        private GameObject _player;
        private SphereCollider _thisCollider;
        private int _currentWayPoint = 0, _state = 0;
        private float[] _waitTimes;

        #endregion

        #region Awake

        void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");

            if (_player != null && thisEnemy != null)
            {
                _active = true;
            }
            else
                Debug.LogError("Enemy Error: Player not found or scriptable object not attached!");


            if (_active)
            {
                _thisCollider = gameObject.GetComponent<SphereCollider>();

                if (_thisCollider != null)
                {
                    _thisCollider.isTrigger = true;
                    _thisCollider.radius = thisEnemy.viewDistance;
                }

                if (GetComponent<NavMeshAgent>() != null)
                {
                    _navMeshAgent = GetComponent<NavMeshAgent>();
                    _navMeshAgent.speed = thisEnemy.walkSpeed;
                    _navMeshAgent.angularSpeed = thisEnemy.walkTurnSpeed;
                    _path = new NavMeshPath();
                }
                else
                {
                    _active = false;
                    Debug.LogError("Enemy Error: No Nav Mesh Agent script attached!");
                }

                if (wayPoints == null)
                {
                    wayPoints = new List<GameObject>();
                }

                if (rotateAtWaypoints)
                    _wayPointRotations = new Vector3[wayPoints.Count];

                _waitTimes = new float[wayPoints.Count];
                for (int i = 0; i < wayPoints.Count; i++)
                {
                    if (wayPoints[i].GetComponent<WayPoint>() != null)
                    {
                        _waitTimes[i] = wayPoints[i].GetComponent<WayPoint>().specificWaitTime;
                        if (rotateAtWaypoints)
                            _wayPointRotations[i] = new Vector3(0, wayPoints[i].GetComponent<WayPoint>().enemyLookDirection, 0);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 0 = Walking, 1 = Running, 2 = Attacking
        /// </summary>
        public void SwitchState(int state)
        {
            if (_navMeshAgent != null && thisEnemy != null)
            {
                if (!thisEnemy.canRun)
                    return;

                switch (state)
                {
                    case 0:
                        _navMeshAgent.speed = thisEnemy.walkSpeed;
                        _navMeshAgent.angularSpeed = thisEnemy.walkTurnSpeed;
                        break;
                    case 1:
                        _navMeshAgent.speed = thisEnemy.runSpeed;
                        _navMeshAgent.angularSpeed = thisEnemy.runTurnSpeed;
                        break;
                    case 2:
                        _navMeshAgent.speed = 0;
                        _active = false;
                        break;
                    default:
                        Debug.LogError("Enemy Error: trying to switch to state that doesn't exist!'");
                        break;
                }

                if (viewConeLight != null)
                {
                    viewConeLight.range = thisEnemy.viewDistance;
                    viewConeLight.color = normalConeColor;
                    viewConeLight.spotAngle = thisEnemy.fieldOfView;
                }
            }
        }

        private void FixedUpdate()
        {
            if (_active)
            {
                if (!_isAggro && wayPoints != null && wayPoints.Count != 0)
                {
                    SwitchState(0); // Switch to walking

                    if (Vector3.Distance(transform.position, wayPoints[_currentWayPoint].transform.position) < wayPointReachRange)
                    {
                        if (!rotateAtWaypoints)
                            _isRotating = false;
                        else
                            _rotatingAtWp = true;

                        UpdatePathRoutine();
                    }

                    if (!_destinationSet)   // If no destination set
                    {
                        _navMeshAgent.SetDestination(wayPoints[_currentWayPoint].transform.position);
                        _rotatingAtWp = false;
                        _destinationSet = true;
                        _isRotating = true;
                    }
                }

                if (_isAggro)
                {
                    if (!_isHugging)
                        SwitchState(1); // Switch to running

                    _navMeshAgent.SetDestination(_lastSighting);
                    _isRotating = true;

                    if (Vector3.Distance(transform.position, _player.transform.position) <
                        thisEnemy.embraceDistance)
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
                    Quaternion lookRotation;
                    float turnSpeed = 1;

                    if (_state == 0)
                        turnSpeed = thisEnemy.walkTurnSpeed;
                    else if (_state == 1)
                        turnSpeed = thisEnemy.runTurnSpeed;

                    if (!_rotatingAtWp)
                        lookRotation = _navMeshAgent.velocity.normalized != Vector3.zero ? Quaternion.LookRotation(_navMeshAgent.velocity.normalized) : transform.rotation;
                    else
                        lookRotation = Quaternion.Euler(_wayPointRotations[_currentWayPoint]) != Quaternion.identity ? Quaternion.Euler(_wayPointRotations[_currentWayPoint]) : transform.rotation;

                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed);

                }

                ViewLightConeControl(); 
            }
        }

        private void UpdatePathRoutine()
        {
            if (!useWaitTime)
            {
                if (loopWaypointRoutine)
                    _currentWayPoint = (_currentWayPoint + 1) % wayPoints.Count;
                else
                {
                    if (_currentWayPoint + 1 > wayPoints.Count)
                        _currentWayPoint = 0;
                    else
                        _currentWayPoint = (_currentWayPoint + 1);
                }

                _destinationSet = false;
            }
            else if (useWaitTime && !_timerRunning)
            {
                StartCoroutine(WaitTimer());
                _timerRunning = true;
            }
        }

        private void OnTriggerStay(Collider col)
        {
            if (_active)
                if (col.tag == _player.tag)
                {
                    Vector3 dir = _player.transform.position - transform.position;
                    float enemyToPlayerAngle = Vector3.Angle(transform.forward, dir);

                    if (enemyToPlayerAngle < thisEnemy.fieldOfView / 2)
                    {
                        RaycastHit hit;

                        if (Physics.Raycast(transform.position + transform.up, dir, out hit, thisEnemy.viewDistance))
                            if (hit.collider.tag == _player.tag)
                            {
                                _lastSighting = _player.transform.position;
                                if (!_isAggro)
                                    StartCoroutine(EnemyAggro());
                            }
                    }
                    else if (HearingPathLength() < thisEnemy.hearingDistance /* && _player.  - TODO: Get player state if walking/sneaking*/)
                    {
                        _lastSighting = _player.transform.position;
                        if (!_isAggro)
                            StartCoroutine(EnemyAggro());
                    }
                    else if (_isAggro)
                    {
                        _lastSighting = _player.transform.position;
                    }
                }
        }

        #region Co-Routines

        private IEnumerator WaitTimer()
        {
            if (_waitTimes[_currentWayPoint] > 0)
                yield return new WaitForSeconds(_waitTimes[_currentWayPoint]);
            else if (useGlobalWaitTime)
                yield return new WaitForSeconds(waitTime);

            _currentWayPoint = (_currentWayPoint + 1) % wayPoints.Count;
            _destinationSet = false;
            _timerRunning = false;
        }

        private IEnumerator EnemyAggro()
        {
            _isAggro = true;
            yield return new WaitForSeconds(thisEnemy.aggroTime);
            _isAggro = false;   // TODO: Make a better solution

            yield return new WaitForSeconds(0.2f);
            if (!_isAggro)
                _destinationSet = false;
        }

        private IEnumerator EnemyHug()
        {
            _isHugging = true;
            SwitchState(2); // Switch to attacking
            yield return new WaitForSeconds(thisEnemy.embraceDelay);

            if (Vector3.Distance(transform.position, _player.transform.position) <
                thisEnemy.embraceDistance)
            {
                Debug.Log("THE PLAYER DIED");
                // _player TODO freeze player for animation

                if (playerDiedEvent != null)
                {
                    playerDiedEvent.Raise();
                }
            }
            else
            {
                _active = true;
                _isHugging = false;
                _isAggro = false;
            }
        }

        private IEnumerator PushDownDelay()
        {
            yield return new WaitForSeconds(thisEnemy.pushedDownDuration);
            _active = true;
        }

        #endregion

        public void PushDown()
        {
            if (thisEnemy != null && _active)
            {
                _active = false;
                StartCoroutine(PushDownDelay());
            }
        }

        private void ViewLightConeControl()
        {
            if (viewConeLight != null)
            {
                if (_isAggro && viewConeLight.color != aggroConeColor)
                    viewConeLight.color = aggroConeColor;
                else if (viewConeLight.color != normalConeColor)
                    viewConeLight.color = normalConeColor;
            }
        }

        private float HearingPathLength()
        {
            if (_navMeshAgent != null)
            {
                _navMeshAgent.CalculatePath(_player.transform.position, _path);

                Vector3[] allPathPoints = new Vector3[_path.corners.Length + 2];
                allPathPoints[0] = transform.position;
                allPathPoints[allPathPoints.Length - 1] = _player.transform.position;

                for (int i = 0; i < _path.corners.Length; i++)
                {
                    allPathPoints[i + 1] = _path.corners[i];
                }

                float tempPathLength = 0.0f;

                for (int i = 0; i < allPathPoints.Length - 1; i++)
                {
                    tempPathLength += Vector3.Distance(allPathPoints[i], allPathPoints[i + 1]);
                }

                return tempPathLength;
            }

            return 0;
        }

        #region Setter and Getters
        public void SetIsActive(bool isActive)
        {
            _active = isActive;
        }

        public bool GetIsActive()
        {
            return _active;
        }
        #endregion

        #region WayPoint System
        public void AddWayPoint()
        {
            GameObject tempWayPointObj;

            if (!GameObject.Find("EnemyWaypoints"))
                new GameObject("EnemyWaypoints");

            if (!GameObject.Find(gameObject.name + "_Waypoints"))
            {
                parentWayPoint = new GameObject(gameObject.name + "_Waypoints");

                parentWayPoint.AddComponent<WayPoint>();
                parentWayPoint.GetComponent<WayPoint>().isParent = true;
                parentWayPoint.transform.parent =
                    GameObject.Find("EnemyWaypoints").transform;
            }
            else
            {
                parentWayPoint = GameObject.Find(gameObject.name + "_Waypoints");
            }

            if (wayPoints == null)
            {
                wayPoints = new List<GameObject>();
            }

            tempWayPointObj = new GameObject("WayPoint" + (wayPoints.Count + 1));
            tempWayPointObj.AddComponent<WayPoint>();
            WayPoint tempWayPointScript = tempWayPointObj.GetComponent<WayPoint>();
            tempWayPointScript.wayPointId = wayPoints.Count + 1;
            tempWayPointScript.parentEnemy = gameObject;
            tempWayPointScript.parentWayPoint = parentWayPoint;

            tempWayPointObj.transform.position = gameObject.transform.position;
            tempWayPointObj.transform.parent = parentWayPoint.transform;
            wayPoints.Add(tempWayPointObj);
        }

        public void RemoveWayPoint()
        {
            if (wayPoints != null)
                if (wayPoints.Count > 0)
                {
                    DestroyImmediate(wayPoints[wayPoints.Count - 1].gameObject);
                }
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
                        Gizmos.DrawWireSphere(wayPoints[i].transform.position, 0.2f);
                        Handles.Label(wayPoints[i].transform.position + (Vector3.up * 0.6f), (i + 1).ToString());

                        Gizmos.color = Color.yellow;
                        if (i + 1 < wayPoints.Count)
                        {
                            Gizmos.DrawLine(wayPoints[i].transform.position, wayPoints[i + 1].transform.position);

                        }
                        else if (i == wayPoints.Count - 1 && wayPoints.Count > 1)
                        {
                            Gizmos.DrawLine(wayPoints[wayPoints.Count - 1].transform.position, wayPoints[0].transform.position);
                        }

                        if (thisEnemy != null)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(transform.position + transform.up, transform.forward * thisEnemy.viewDistance + (transform.position + transform.up));
                        }
                    }
                }
        }
#endif
        #endregion
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

            EditorGUILayout.HelpBox("Please only use the 'Add WayPoint' button to create new waypoints. They can then be found in the 'WayPoints' object in the hierarchy.", MessageType.Info);

            DrawDefaultInspector(); // for other non-HideInInspector fields

            var script = target as Enemy;

            if (script.useWaitTime)
            {
                script.useGlobalWaitTime = EditorGUILayout.Toggle("Use Global Wait Time?", script.useGlobalWaitTime);
                EditorGUILayout.HelpBox("If a wait time is specified on the waypoint it will override the global wait time value", MessageType.None);
            }
            if (script.useGlobalWaitTime && script.useWaitTime)
                script.waitTime = EditorGUILayout.FloatField("Global Wait Time", script.waitTime);

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
                script.AddWayPoint();
            }

            if (GUILayout.Button("Remove WayPoint"))
            {
                script.RemoveWayPoint();
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
