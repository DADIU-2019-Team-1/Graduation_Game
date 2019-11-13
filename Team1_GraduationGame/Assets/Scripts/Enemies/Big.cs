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
        private List<GameObject> _spawnPoints;
        private GameObject _parentSpawnPoint;
        private bool _isSpawned, _isRotating, _turnRight;
        private int _layerMask;
        private Vector3 _lookRange;
        private Quaternion _lookRotation;

        // Public:
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

            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                if (_spawnPoints[i].GetComponent<WayPoint>() != null)
                {
                    _lookRange = new Vector3(0, _spawnPoints[i].GetComponent<Big_SpawnPoint>().lookRange, 0);
                }
            }

        }

        private void Start()
        {

        }

        private void FixedUpdate()
        {
            if (_isRotating)    // TODO make this work
            {
                if (_turnRight)
                    _lookRotation = Quaternion.Euler(_lookRange) != Quaternion.identity ? Quaternion.Euler(_lookRange) : transform.rotation;
                else
                    _lookRotation = Quaternion.Euler(_lookRange) != Quaternion.identity ? Quaternion.Euler(_lookRange) : transform.rotation;
                
                transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, headRotateSpeed);
            }

            if (_player != null && _spawnPoints != null)
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

                if (_spawnPoints == null)
                {
                    _spawnPoints = new List<GameObject>();
                }

                tempWayPointObj = new GameObject("SpawnPoint" + (_spawnPoints.Count + 1));
                tempWayPointObj.AddComponent<Big_SpawnPoint>();
                Big_SpawnPoint tempWayPointScript = tempWayPointObj.GetComponent<Big_SpawnPoint>();
                tempWayPointScript.parentObject = _parentSpawnPoint;

                tempWayPointObj.transform.position = gameObject.transform.position;
                tempWayPointObj.transform.parent = _parentSpawnPoint.transform;
                _spawnPoints.Add(tempWayPointObj);
            }
        }

        public void RemoveSpawnPoint()
        {
            if (Application.isEditor)
            {
                if (_spawnPoints != null)
                    if (_spawnPoints.Count > 0)
                    {
                        DestroyImmediate(_spawnPoints[_spawnPoints.Count - 1].gameObject);
                    }
            }
        }

        private void OnDrawGizmos()
        {
            if (drawGizmos && Application.isEditor)
                if (_spawnPoints != null)
                {
                    for (int i = 0; i < _spawnPoints.Count; i++)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireSphere(_spawnPoints[i].transform.position, 0.3f);
                        Handles.Label(_spawnPoints[i].transform.position + (Vector3.up * 0.6f), (i + 1).ToString());

                    }
                }
        }
        #endregion
#endif
    }
}