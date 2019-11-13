using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Team1_GraduationGame.Enemies
{
    [ExecuteInEditMode]
    public class BigLookPoint : MonoBehaviour
    {
        [HideInInspector] public GameObject parentBig;
        [HideInInspector] public GameObject parentLookPoint;
        [HideInInspector] public bool isParent = false;
        [Range(0.0f, 10.0f)] public float specificWaitTime = 0.0f;
        public int lookPointId;

#if UNITY_EDITOR
        void OnDestroy()
        {
            if (parentBig != null && !isParent && Application.isEditor)
            {
                if (parentBig.GetComponent<Big>() != null && lookPointId != 0)
                {
                    Big tempBig = parentBig.GetComponent<Big>();

                    if (tempBig.lookPoints?.Count > 0 && tempBig.lookPoints.ElementAtOrDefault(lookPointId - 1))
                    {
                        tempBig.lookPoints.RemoveAt(lookPointId - 1);

                        for (int i = 0; i < tempBig.lookPoints.Count; i++)
                        {
                            if (tempBig.lookPoints[i].GetComponent<BigLookPoint>() != null)
                            {
                                tempBig.lookPoints[i].GetComponent<BigLookPoint>().lookPointId = i + 1;
                                tempBig.lookPoints[i].name = "LookPoint" + (i + 1);
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
    [CustomEditor(typeof(BigLookPoint))]
    public class BigLookPoint_Inspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var script = target as BigLookPoint;

            if (!script.isParent)
            {
                DrawDefaultInspector(); // for other non-HideInInspector fields
            }
            else
            {
                EditorGUILayout.LabelField("//// THIS IS A PARENT LOOKPOINT ////");
            }

        }
    }
#endif

}