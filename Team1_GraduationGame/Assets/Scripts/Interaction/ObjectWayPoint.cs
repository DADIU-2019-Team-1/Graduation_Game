using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

namespace Team1_GraduationGame.Interaction
{
    [ExecuteInEditMode]
    public class ObjectWayPoint : MonoBehaviour
    {
        [HideInInspector] public GameObject parentObject;
        [HideInInspector] public GameObject parentWayPoint;
        [HideInInspector] public bool isParent = false;
        public int wayPointId;


        void OnDestroy()
        {
            if (parentObject != null && !isParent && Application.isEditor)
            {
                if (parentObject.GetComponent<ObjectPush>() != null && wayPointId != 0)
                {
                    ObjectPush tempObject = parentObject.GetComponent<ObjectPush>();

                    if (tempObject.wayPoints.Count > 0 && tempObject.wayPoints.ElementAtOrDefault(wayPointId - 1))
                    {
                        tempObject.wayPoints.RemoveAt(wayPointId - 1);

                        for (int i = 0; i < tempObject.wayPoints.Count; i++)
                        {
                            if (tempObject.wayPoints[i].GetComponent<ObjectWayPoint>() != null)
                            {
                                tempObject.wayPoints[i].GetComponent<ObjectWayPoint>().wayPointId = i + 1;
                                tempObject.wayPoints[i].name = "WayPoint" + (i + 1);
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
    }

    [CustomEditor(typeof(ObjectWayPoint))]
    public class ObjectWayPoint_Inspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var script = target as ObjectWayPoint;

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
}
#endif