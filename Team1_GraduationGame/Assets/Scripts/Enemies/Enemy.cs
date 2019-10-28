using UnityEngine.AI;

namespace Team1_GraduationGame.Enemies
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(SphereCollider))]
    public class Enemy : MonoBehaviour
    {
        // Scriptable Object References:
        public BaseEnemy thisEnemy;

        // Public variables:
        public Transform[] wayPoints;
        [Tooltip("How close the enemy should be to the waypoint for it to switch to the next")] public float wayPointReachRange = 1;

        // Private variables:
        private bool _active;
        private NavMeshAgent _navMeshAgent;
        private Vector3 _lastSighting;
        private GameObject _player;
        private SphereCollider _thisCollider;
        private int currentWayPoint = 0;

        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");

            if (_player != null && thisEnemy != null)
            {
                _active = true;
            }

            if (_active)
            {
                _thisCollider = gameObject.GetComponent<SphereCollider>();

                if (_thisCollider != null)
                {
                    _thisCollider.isTrigger = true;
                    _thisCollider.radius = thisEnemy.viewDistance;
                }
            }

            if (GetComponent<NavMeshAgent>() != null)
            {
                _navMeshAgent = GetComponent<NavMeshAgent>();
                _navMeshAgent.speed = thisEnemy.speed;
                _navMeshAgent.angularSpeed = thisEnemy.turnSpeed;
                //_navMeshAgent.acceleration = thisEnemy.accelerationTime;
            }
            else
            {
                Debug.LogError("Enemy Error: Please attach Nav Mesh Agent component to enemy for pathfinding to work");
            }
        }

        private void FixedUpdate()
        {
            if (wayPoints != null)
            {
                // transform.position = Vector3.Lerp(_lastPos.position, wayPoints[currentWayPoint].position, thisEnemy.speed * Time.fixedDeltaTime);
                transform.position += (wayPoints[currentWayPoint].position - transform.position).normalized *
                                      thisEnemy.speed * Time.fixedDeltaTime;

                if (Vector3.Distance(transform.position, wayPoints[currentWayPoint].position) < wayPointReachRange)
                {
                    currentWayPoint = (currentWayPoint + 1) % wayPoints.Length;
                }

            }
        }

        private void OnTriggerStay(Collider col)
        {
            if (col.tag == _player.tag)
            {
                Vector3 dir = _player.transform.position - transform.position;
                float enemyToPlayerAngle = Vector3.Angle(gameObject.transform.forward, dir);

                if (enemyToPlayerAngle < thisEnemy.viewAngleRange / 2)
                {
                    RaycastHit hit;

                    if (Physics.Raycast(transform.position + transform.up, dir, out hit, thisEnemy.viewDistance))
                        if (hit.collider.gameObject == _player)
                        {
                            UpdateLastSighting(_player.transform.position);
                            // TODO: Change waypoint to last sigthing
                        }
                }
            }
        }

        private void UpdateLastSighting(Vector3 position)
        {
            _lastSighting = position;
        }

    }

}
