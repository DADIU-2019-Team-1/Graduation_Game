#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team1_GraduationGame.Editor;

namespace Team1_GraduationGame.Enemies
{
    [ExecuteInEditMode]
    public class WayPoint : MonoBehaviour
    {
        public GameObject parentEnemy;
        public GameObject parentWayPoint;
        public bool isParent = false;
        public int wayPointId;

        void OnDestroy()
        {
            if (parentEnemy != null && !isParent)
            {
                if (parentEnemy.GetComponent<Enemy>() != null && wayPointId != 0)
                {
                    Enemy tempEnemy = parentEnemy.GetComponent<Enemy>();

                    tempEnemy.wayPoints.RemoveAt(wayPointId - 1);

                    for (int i = 0; i < tempEnemy.wayPoints.Count; i++)
                    {
                        if (tempEnemy.wayPoints[i].gameObject.GetComponent<WayPoint>() != null)
                        {
                            tempEnemy.wayPoints[i].gameObject.GetComponent<WayPoint>().wayPointId = i + 1;
                            tempEnemy.wayPoints[i].name = "WayPoint" + (i + 1);
                        }
                    }

                    //if (tempEnemy.wayPoints.Count == 0 && parentWayPoint != null)
                    //{
                    //    DestroyImmediate(parentWayPoint);
                    //}
                }
            }
        }

        private void Update()
        {
            if (isParent)
            {
                if (transform.childCount == 0)
                {
                    DestroyImmediate(gameObject);
                }
            }
        }
    }

}
#endif
