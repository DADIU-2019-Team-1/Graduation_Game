using UnityEngine.AI;

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
        public Transform[] wayPoints;

        [Tooltip("How close the enemy should be to the waypoint for it to switch to the next")]
        public float wayPointReachRange = 1;

        // Private variables:
        private bool _active;
        private NavMeshAgent _navMeshAgent;
        private Vector3 _lastSighting;
        private GameObject _player;
        private SphereCollider _thisCollider;
        private int currentWayPoint = 0;

        void Awake()
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

            _navMeshAgent = GetComponent<NavMeshAgent>();

            if (GetComponent<NavMeshAgent>() != null)
            {
                _navMeshAgent.speed = thisEnemy.speed;
                _navMeshAgent.angularSpeed = thisEnemy.turnSpeed;
                //_navMeshAgent.acceleration = thisEnemy.accelerationTime;
            }
        }

        private void FixedUpdate()
        {
            if (wayPoints != null)
            {
                //transform.position += (wayPoints[currentWayPoint].position - transform.position).normalized *
                //                      thisEnemy.speed * Time.fixedDeltaTime;

                _navMeshAgent.SetDestination(wayPoints[currentWayPoint].position);

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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (wayPoints != null)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < wayPoints.Length; i++)
                {
                    Gizmos.DrawWireSphere(wayPoints[i].position, 0.2f);
                }
            }

        }
#endif

    }
}
