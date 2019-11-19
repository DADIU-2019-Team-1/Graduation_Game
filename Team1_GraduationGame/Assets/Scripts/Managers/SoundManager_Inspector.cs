// Script by Jakob Elkjær Husted
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Team1_GraduationGame.Managers
{

#if UNITY_EDITOR
    [CustomEditor(typeof(SoundManager))]
    public class SoundManager_Inspector : UnityEditor.Editor
    {
        private GUIStyle headerStyle;

        public override void OnInspectorGUI()
        {
            #region Header Style
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle();
                headerStyle.fontSize = 11;
                headerStyle.normal.textColor = Color.white;
                headerStyle.border = new RectOffset(5, 5, 20, 12);
                headerStyle.alignment = TextAnchor.MiddleCenter;
                headerStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/flow background.png") as Texture2D;
            }
            #endregion

            EditorGUILayout.HelpBox("Note: This requires Wwise to work", MessageType.None);

            var script = target as SoundManager;

            if (script.gameObject.GetComponent<Collider>() == null)
            {
                SerializedProperty colliderSelectProp = serializedObject.FindProperty("attachCollider");
                EditorGUILayout.PropertyField(colliderSelectProp);
                serializedObject.ApplyModifiedProperties();

                if ((int)script.attachCollider != 0)
                {
                    script.AttachCollider((int)script.attachCollider);
                    script.attachCollider = SoundManager.colliderTypes.None;
                }
            }

            DrawDefaultInspector(); // for other non-HideInInspector fields

            if (GUILayout.Button("Stop All"))
            {
                script.StopAll();
            }

            if (script.soundEvents != null)
                for (int i = 0; i < script.soundEvents.Length; i++)
                {
                    if (Application.isEditor && script.soundEvents[i].soundManagerGameObject != script.gameObject)
                    {
                        script.soundEvents[i].soundManagerGameObject = script.gameObject;
                    }

                    EditorGUILayout.Space();
                    string tempTitleText = script.soundEvents[i].behaviorSelector + " | " + script.soundEvents[i].triggerTypeSelector;

                    if (script.soundEvents[i].isActive)
                        headerStyle.fontStyle = FontStyle.Bold;
                    else
                        headerStyle.fontStyle = FontStyle.Normal;


                    if (GUILayout.Button(tempTitleText, headerStyle))
                    {
                        script.soundEvents[i].isActive = !script.soundEvents[i].isActive;
                    }

                    if (script.soundEvents[i].isActive)
                    {
                        DrawUILine(false);

                        SerializedProperty triggerEventSelectorProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].triggerTypeSelector");
                        EditorGUILayout.PropertyField(triggerEventSelectorProp);

                        if ((int)script.soundEvents[i].triggerTypeSelector == 0)
                        {
                            SerializedProperty triggerEventProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].triggerEvent");
                            EditorGUILayout.PropertyField(triggerEventProp);
                        }
                        else if ((int)script.soundEvents[i].triggerTypeSelector == 1)
                        {
                            SerializedProperty triggerFloatEventProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].triggerFloatEvent");
                            EditorGUILayout.PropertyField(triggerFloatEventProp);
                        }
                        else if ((int)script.soundEvents[i].triggerTypeSelector == 2 ||
                                 (int)script.soundEvents[i].triggerTypeSelector == 3)
                        {
                            script.soundEvents[i].checkForTag = EditorGUILayout.Toggle("Check for tag?",
                                script.soundEvents[i].checkForTag);
                            if (script.soundEvents[i].checkForTag)
                            {
                                script.soundEvents[i].tag = EditorGUILayout.TextField("Tag", script.soundEvents[i].tag);
                            }
                        }
                        else if ((int) script.soundEvents[i].triggerTypeSelector == 6)
                        {
                            SerializedProperty triggerIntEventProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].triggerIntEvent");
                            EditorGUILayout.PropertyField(triggerIntEventProp);
                        }

                        script.soundEvents[i].runOnce = EditorGUILayout.Toggle("Run Once", script.soundEvents[i].runOnce);

                        script.soundEvents[i].triggerDelay =
                            EditorGUILayout.FloatField("Trigger Delay", script.soundEvents[i].triggerDelay);

                        SerializedProperty behaviorSelectorProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].behaviorSelector");
                        EditorGUILayout.PropertyField(behaviorSelectorProp);

                        DrawUILine(false);

                        EditorGUI.indentLevel = EditorGUI.indentLevel + 2;

                        ////// BEHAVIORS: //////
                        #region ///// 0 Event Behavior & 4 Ambient Behavior //////
                        if ((int)script.soundEvents[i].behaviorSelector == 0 || (int)script.soundEvents[i].behaviorSelector == 4)
                        {
                            if ((int)script.soundEvents[i].behaviorSelector == 4)
                            {
                                EditorGUILayout.HelpBox("Ambient feature is not yet implemented!", MessageType.Warning);
                            }

                            script.soundEvents[i].useOtherGameObject = EditorGUILayout.Toggle("Use Other GameObj?",
                                script.soundEvents[i].useOtherGameObject);

                            if (script.soundEvents[i].useOtherGameObject)
                            {
                                SerializedProperty targetGameObjectProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].targetGameObject");
                                EditorGUILayout.PropertyField(targetGameObjectProp);
                            }

                            SerializedProperty eventDataProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].wwiseEvent");
                            EditorGUILayout.PropertyField(eventDataProp);

                            script.soundEvents[i].useActionOnEvent = EditorGUILayout.Toggle("Do Action On Event?", script.soundEvents[i].useActionOnEvent);
                            script.soundEvents[i].useCallbacks = EditorGUILayout.Toggle("Use Callbacks?", script.soundEvents[i].useCallbacks);

                            if (script.soundEvents[i].useActionOnEvent)
                            {
                                DrawUILine(false);

                                SerializedProperty actionOnEventTypeProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].actionOnEventType");
                                EditorGUILayout.PropertyField(actionOnEventTypeProp);

                                SerializedProperty curveInterpolationProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].curveInterpolation");
                                EditorGUILayout.PropertyField(curveInterpolationProp);

                                script.soundEvents[i].transitionDuration = EditorGUILayout.Slider("Fade Time (secs):", script.soundEvents[i].transitionDuration, 0.0f, 20.0f); // Note: standard was up to 60 sec. 120 might not work?
                            }

                            if (script.soundEvents[i].useCallbacks)
                            {
                                DrawUILine(false);

                                for (int j = 0; j < script.soundEvents[i].callbacks.Count; j++)
                                {
                                    SerializedProperty callbackFlagProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].callbacks.Array.data[" + j + "].Flags");
                                    SerializedProperty callbackObjProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].callbacks.Array.data[" + j + "].GameObject");
                                    EditorGUILayout.PropertyField(callbackFlagProp);
                                    EditorGUILayout.PropertyField(callbackObjProp);
                                    script.soundEvents[i].callbacks[j].FunctionName =
                                        EditorGUILayout.TextField("Function Name:",
                                            script.soundEvents[i].callbacks[j].FunctionName);
                                }

                                if (GUILayout.Button("Add"))
                                {
                                    script.soundEvents[i].callbacks.Add(new SoundEvent.CallbackData());
                                }
                                if (GUILayout.Button("Delete"))
                                {
                                    if (script.soundEvents[i].callbacks.Count != 0)
                                        script.soundEvents[i].callbacks.RemoveAt(script.soundEvents[i].callbacks.Count - 1);
                                }
                            }

                            if ((int)script.soundEvents[i].behaviorSelector == 4)
                            {
                                DrawUILine(false);

                                SerializedProperty multiPositionTypeProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].multiPositionType");
                                EditorGUILayout.PropertyField(multiPositionTypeProp);

                                SerializedProperty multiPosTypeLabelProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].multiPosTypeLabel");
                                EditorGUILayout.PropertyField(multiPosTypeLabelProp);
                            }

                            DrawUILine(false);

                            if (GUILayout.Button("Play"))
                            {
                                script.soundEvents[i].PlayWwiseEvent();
                            }
                            if (GUILayout.Button("Stop"))
                            {
                                script.soundEvents[i].StopWwiseEvent();
                            }

                        }
                        #endregion
                        #region ///// 1 State Behavior //////
                        else if ((int)script.soundEvents[i].behaviorSelector == 1)
                        {
                            SerializedProperty stateProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].wwiseState");
                            EditorGUILayout.PropertyField(stateProp);
                        }
                        #endregion
                        #region ///// 2 Switch Behavior /////
                        else if ((int)script.soundEvents[i].behaviorSelector == 2)
                        {
                            SerializedProperty switchProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].wwiseSwitch");
                            EditorGUILayout.PropertyField(switchProp);
                        }
                        #endregion
                        #region ///// 3 RTPC Behavior /////
                        else if ((int)script.soundEvents[i].behaviorSelector == 3)
                        {

                            SerializedProperty rtpcProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].wwiseRTPC");
                            EditorGUILayout.PropertyField(rtpcProp);

                            script.soundEvents[i].useOtherGameObject = EditorGUILayout.Toggle("Use Other GameObj?",
                                script.soundEvents[i].useOtherGameObject);

                            if (script.soundEvents[i].useOtherGameObject)
                            {
                                SerializedProperty targetGameObjectProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].targetGameObject");
                                EditorGUILayout.PropertyField(targetGameObjectProp);
                            }

                            script.soundEvents[i].objDistanceToRtpc = EditorGUILayout.Toggle("Obj Distance to RTPC?",
                                script.soundEvents[i].objDistanceToRtpc);

                            if (script.soundEvents[i].objDistanceToRtpc)
                            {
                                SerializedProperty secondGameObjectProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].secondGameObject");
                                EditorGUILayout.PropertyField(secondGameObjectProp);
                                script.soundEvents[i].setCustomRtpcFloat = false;
                            }

                            script.soundEvents[i].rtpcGlobal = EditorGUILayout.Toggle("Use Global RTPC", script.soundEvents[i].rtpcGlobal);

                            if (!script.soundEvents[i].objDistanceToRtpc)
                            {

                                EditorGUILayout.Space();

                                EditorGUILayout.HelpBox("TRUE: Value controls RTPC  /  FALSE: RTPC controls scriptable object",
                                    MessageType.None);
                                script.soundEvents[i].rtpcRoleBool =
                                    EditorGUILayout.Toggle("Set RTPC role",
                                        script.soundEvents[i].rtpcRoleBool);

                                if ((int)script.soundEvents[i].triggerTypeSelector == 1 && !script.soundEvents[i].setCustomRtpcFloat && script.soundEvents[i].rtpcRoleBool)
                                {
                                    script.soundEvents[i].useValueFromEvent = EditorGUILayout.Toggle("Use value from event?",
                                        script.soundEvents[i].useValueFromEvent);
                                }

                                if (!script.soundEvents[i].useValueFromEvent && script.soundEvents[i].rtpcRoleBool)
                                {
                                    script.soundEvents[i].setCustomRtpcFloat =
                                        EditorGUILayout.Toggle("Use custom value?",
                                            script.soundEvents[i].setCustomRtpcFloat);
                                }
                                else
                                {
                                    script.soundEvents[i].setCustomRtpcFloat = false;
                                }

                                if (script.soundEvents[i].setCustomRtpcFloat && !script.soundEvents[i].useValueFromEvent)
                                {
                                    script.soundEvents[i].customValue =
                                        EditorGUILayout.FloatField("Float Value", script.soundEvents[i].customValue);
                                    script.soundEvents[i].useValueFromEvent = false;
                                }

                                if (!script.soundEvents[i].setCustomRtpcFloat &&
                                    !script.soundEvents[i].useValueFromEvent || !script.soundEvents[i].rtpcRoleBool)
                                {
                                    EditorGUILayout.Space();

                                    SerializedProperty scriptableObjectProp =
                                        serializedObject.FindProperty(
                                            "soundEvents.Array.data[" + i + "].rtpcScriptableObject");
                                    EditorGUILayout.PropertyField(scriptableObjectProp);
                                }

                            }
                        }
                    }
                    #endregion

                    EditorGUI.indentLevel = EditorGUI.indentLevel - 2;
                    DrawUILine(true);
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

}
