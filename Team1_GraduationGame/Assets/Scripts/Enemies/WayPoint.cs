// Script by Jakob Elkjær Husted
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Team1_GraduationGame.Enemies
{
    [ExecuteInEditMode]
    public class WayPoint : MonoBehaviour
    {
        [HideInInspector] public GameObject parentEnemy;
        [HideInInspector] public GameObject parentWayPoint;
        [HideInInspector] public bool isParent = false;
        [Range(0.0f, 20.0f)] public float specificWaitTime = 0.0f;
        [Range(0.0f, 359.0f)] public float enemyLookDirection = 0.0f;
        public int wayPointId;

#if UNITY_EDITOR
        void OnDestroy()
        {
            if (parentEnemy != null && !isParent && Application.isEditor)
            {
                if (parentEnemy.GetComponent<Enemy>() != null && wayPointId != 0)
                {
                    Enemy tempEnemy = parentEnemy.GetComponent<Enemy>();

                    if (tempEnemy.wayPoints.Count > 0 && tempEnemy.wayPoints.ElementAtOrDefault(wayPointId - 1))
                    {
                        tempEnemy.wayPoints.RemoveAt(wayPointId - 1);

                        for (int i = 0; i < tempEnemy.wayPoints.Count; i++)
                        {
                            if (tempEnemy.wayPoints[i].GetComponent<WayPoint>() != null)
                            {
                                tempEnemy.wayPoints[i].GetComponent<WayPoint>().wayPointId = i + 1;
                                tempEnemy.wayPoints[i].name = "WayPoint" + (i + 1);
                            }
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (isParent && Application.isEditor)
            {
                if (transform.childCount == 0)
                {
                    DestroyImmediate(gameObject);
                }
            }
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WayPoint))]
    public class WayPoint_Inspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var script = target as WayPoint;

            if (!script.isParent)
            {
                DrawDefaultInspector(); // for other non-HideInInspector fields
            }
            else
            {
                EditorGUILayout.LabelField("//// THIS IS A PARENT WAYPOINT ////");
            }

        }
    }
#endif
}