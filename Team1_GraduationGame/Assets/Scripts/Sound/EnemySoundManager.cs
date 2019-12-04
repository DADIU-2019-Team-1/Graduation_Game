// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.Sound
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Team1_GraduationGame.Enemies;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [RequireComponent(typeof(AkGameObj))]
    public class EnemySoundManager : MonoBehaviour
    {
        public bool useRtpc = false;
        [HideInInspector] public bool useGlobalRtpcs = false;

        // Wwise:
        [HideInInspector] public AK.Wwise.RTPC speedRTPC;
        public bool usePushedDown = true, useGettingUp = true, useOnset = true, useHold = true, useSpot = true;
        public AK.Wwise.Event attackingPlayerEvent, pushedDownEvent, gettingUpEvent, onsetEvent, holdEvent, spotEvent;  // TODO make sure all these vents gets raised

        private Enemy _thisEnemy;

        private void Awake()
        {
            if (gameObject.GetComponent<Enemy>())
            {
                _thisEnemy = gameObject.GetComponent<Enemy>();
                InvokeRepeating("CustomUpdate", 0.3f, 0.7f);
            }
        }

        private void CustomUpdate()
        {
            if (useRtpc)
            {
                if (speedRTPC != null)
                {
                    if (useGlobalRtpcs)
                        speedRTPC.SetGlobalValue(_thisEnemy.GetSpeed());
                    else
                        AkSoundEngine.SetRTPCValue(speedRTPC.Id, _thisEnemy.GetSpeed());
                }
                //if (stateRTPC != null)
                //{
                //    if (useGlobalRtpcs)
                //        stateRTPC.SetGlobalValue(_thisEnemy.GetState());
                //    else
                //        AkSoundEngine.SetRTPCValue(stateRTPC.Id, _thisEnemy.GetState());
                //}
            }
        }

        public void AttackPlayer()
        {
            attackingPlayerEvent?.Post(gameObject);
        }
        public void PushedDown()
        {
            if (usePushedDown)
                pushedDownEvent?.Post(gameObject);
        }
        public void GettingUp()
        {
            if (useGettingUp)
                gettingUpEvent?.Post(gameObject);
        }
        public void Spotted()
        {
            if (useSpot)
                spotEvent?.Post(gameObject);
        }
        public void Onset()
        {
            if (useOnset)
                onsetEvent?.Post(gameObject);
        }
        public void Hold()
        {
            if (useHold)
                holdEvent?.Post(gameObject);
        }
    }

#if UNITY_EDITOR
    #region Custom Inspector
    [CustomEditor(typeof(EnemySoundManager))]
    public class EnemySoundManager_Inspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script = target as EnemySoundManager;

            if (script.useRtpc)
            {
                script.useGlobalRtpcs = EditorGUILayout.Toggle("Use global RTPC?", script.useGlobalRtpcs);

                SerializedProperty speedRTPCProp = serializedObject.FindProperty("speedRTPC");
                EditorGUILayout.PropertyField(speedRTPCProp);

                //SerializedProperty stateRTPCProp = serializedObject.FindProperty("stateRTPC");
                //EditorGUILayout.PropertyField(stateRTPCProp);

                serializedObject.ApplyModifiedProperties();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
            }
        }
    }
    #endregion
#endif
}