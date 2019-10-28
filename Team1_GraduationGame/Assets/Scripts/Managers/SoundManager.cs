using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Team1_GraduationGame.Events;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

// NOTE: This script requires Wwise to work //

namespace Team1_GraduationGame.Managers
{
    public class SoundManager : MonoBehaviour
    {
        public SoundEvent[] soundEvents;

        private void Awake()
        {
            if (soundEvents != null)
                for (int i = 0; i < soundEvents.Length; i++)
                {
                    if ((int) soundEvents[i].triggerTypeSelector == 0)
                    {
                        soundEvents[i].soundEventListener = new SoundVoidEventListener();
                        soundEvents[i].soundEventListener.GameEvent = soundEvents[i].triggerEvent;
                        soundEvents[i].soundEventListener.SoundEventClass = soundEvents[i];
                        soundEvents[i].soundEventListener.Enable();
                    } 
                    else if ((int) soundEvents[i].triggerTypeSelector == 1)
                    {
                        soundEvents[i].soundFloatEventListener = new SoundFloatEventListener();
                        soundEvents[i].soundFloatEventListener.GameEvent = soundEvents[i].triggerFloatEvent;
                        soundEvents[i].soundFloatEventListener.SoundEventClass = soundEvents[i];
                        soundEvents[i].soundFloatEventListener.Enable();
                    }

                    soundEvents[i].thisSoundManager = this;
                    soundEvents[i].soundEventId = i;
                    soundEvents[i].soundManagerGameObject = gameObject;
                }
        }

        public void StartCoroutine(int id)
        {
            if (soundEvents[id] != null)
                StartCoroutine(soundEvents[id].WaitForDelay());
        }

        public void StopAll()
        {
            Debug.Log("Stopping All Sounds");
            AkSoundEngine.StopAll();
        }
    }

    #region Sound Event Container
    [System.Serializable]
    public class SoundEvent
    {
        #region Class Variables
        [HideInInspector] public SoundManager thisSoundManager;
        [HideInInspector] public int soundEventId;

        // Event system:
        public enum EventTypeEnum
        {
            VoidEvent,
            FloatEvent,
            OnTriggerEnter,
            OnTriggerExit,
            Start
        }
        [HideInInspector] public EventTypeEnum triggerTypeSelector;
        [HideInInspector] public SoundVoidEventListener soundEventListener;
        [HideInInspector] public SoundFloatEventListener soundFloatEventListener;
        [HideInInspector] public float triggerDelay = 0.0f;
        [HideInInspector] public VoidEvent triggerEvent;
        [HideInInspector] public FloatEvent triggerFloatEvent;
        [HideInInspector] public bool fromScriptableObjToRtpc;
        private bool _eventFired = false;
        private float _parsedValue = 0;
        [HideInInspector] public float customValue, transitionDuration = 0.0f;

        // Behavior switching:
        public enum BehaviorEnum
        {
            Event,
            State,
            Switch,
            RTPC,
            Ambient
        }

        [HideInInspector] public BehaviorEnum behaviorSelector;

        // GameObjects:
        [HideInInspector] public GameObject soundManagerGameObject, targetGameObject;

        // Wwise:
        private AkEventCallbackMsg EventCallbackMsg = new AkEventCallbackMsg();
        [HideInInspector] public List<CallbackData> callbacks;
        [HideInInspector] public AK.Wwise.Event wwiseEvent = new AK.Wwise.Event();
        [HideInInspector] public AK.Wwise.State wwiseState;
        [HideInInspector] public AK.Wwise.Switch wwiseSwitch;
        [HideInInspector] public AK.Wwise.RTPC wwiseRTPC;
        [HideInInspector] public FloatVariable rtpcScriptableObject;
        [HideInInspector] public AkActionOnEventType actionOnEventType = AkActionOnEventType.AkActionOnEventType_Stop;
        [HideInInspector] public AkCurveInterpolation curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear;
        [HideInInspector] public AkMultiPositionType multiPositionType = AkMultiPositionType.MultiPositionType_MultiSources;
        [HideInInspector] public MultiPositionTypeLabel multiPosTypeLabel = MultiPositionTypeLabel.Simple_Mode;

        // Bools:
        [HideInInspector] public bool useCallbacks, useActionOnEvent, rtpcGlobal, runOnce, useOtherGameObject, setCustomRtpcFloat, isActive;
        #endregion

        #region Wwise play/stop events
        public void PlayWwiseEvent()
        {

            if (wwiseEvent != null && (int) triggerTypeSelector == 0)
            {
                Debug.Log("Playing Wwise Event");
                if (targetGameObject == null || !useOtherGameObject)
                {
                    wwiseEvent.Post(soundManagerGameObject);
                    if (targetGameObject == null && useOtherGameObject)
                        Debug.LogError("SoundManager: Target GameObject is not set! - Playing event from default object instead");
                }
                else
                    wwiseEvent.Post(targetGameObject);
            }
        }

        public void StopWwiseEvent()
        {
            if (wwiseEvent != null && (int) triggerTypeSelector == 0)
            {
                Debug.Log("Stopping Wwise Event");
                if (targetGameObject == null || !useOtherGameObject)
                    wwiseEvent.Stop(soundManagerGameObject);
                else
                    wwiseEvent.Stop(targetGameObject);
            }
        }
        #endregion

        public void EventRaised(float value)
        {
            _parsedValue = value;

            if (!_eventFired)
            {
                if (triggerDelay <= 0)
                    SelectBehavior();
                else if (triggerDelay > 0)
                    thisSoundManager.StartCoroutine(soundEventId);
            }

            if (runOnce)
            {
                _eventFired = true;
            }
        }

        private void SelectBehavior()
        {
            switch ((int)behaviorSelector)
            {
                case 0:
                    eventHandler();
                    break;
                case 1:
                    wwiseState.SetValue();
                    break;
                case 2:
                    wwiseSwitch.SetValue(soundManagerGameObject);
                    break;
                case 3:
                    RTPCHandler();
                    break;
                case 4:
                    AmbientHandler();
                    break;
                default:
                    break;
            }
        }

        private void AmbientHandler()   // TODO
        {

        }

        private float GetDistanceBetweenObjects()
        {
            float distance = Vector3.Distance(targetGameObject.transform.position, soundManagerGameObject.transform.position);
            return distance;
        }

        #region Event (Wwise event) Handler
        private void eventHandler()
        {
            if (useActionOnEvent)
            {
                if (targetGameObject == null || !useOtherGameObject)
                {
                    wwiseEvent.ExecuteAction(soundManagerGameObject, actionOnEventType, (int)transitionDuration * 1000, curveInterpolation);
                    if (targetGameObject == null && useOtherGameObject)
                        Debug.LogWarning("SoundManager: Target GameObject is not set! - Using default object instead");
                }
                else
                    wwiseEvent.ExecuteAction(targetGameObject, actionOnEventType, (int)transitionDuration * 1000, curveInterpolation);

                return;
            }

            if (useCallbacks)
                for (int i = 0; i < callbacks.Count; i++)
                {
                    wwiseEvent.Post(callbacks[i].GameObject, callbacks[i].Flags, Callback);
                }
            else
            {
                PlayWwiseEvent();
            }

        }
        #endregion

        #region RTPC Handler
        private void RTPCHandler()
        {
            if (rtpcScriptableObject != null && wwiseRTPC != null)
            {
                if (fromScriptableObjToRtpc)
                {
                    if (rtpcGlobal)
                    {
                        wwiseRTPC.SetGlobalValue(rtpcScriptableObject.value);
                    }
                    else
                    {
                        if (targetGameObject == null || !useOtherGameObject)
                        {
                            wwiseRTPC.SetValue(soundManagerGameObject, rtpcScriptableObject.value);
                            if (targetGameObject == null && useOtherGameObject)
                                Debug.LogWarning("SoundManager: Target GameObject is not set! - Using default object instead");
                        }
                        else
                            wwiseRTPC.SetValue(targetGameObject, rtpcScriptableObject.value);
                    }
                }
                else
                {
                    if (rtpcGlobal)
                    {
                        rtpcScriptableObject.value = wwiseRTPC.GetGlobalValue();
                    }
                    else
                    {
                        if (targetGameObject == null || !useOtherGameObject)
                        {
                            rtpcScriptableObject.value = wwiseRTPC.GetValue(targetGameObject);
                            if (targetGameObject == null && useOtherGameObject)
                                Debug.LogWarning("SoundManager: Target GameObject is not set! - Using default object instead");
                        }
                        else
                            rtpcScriptableObject.value = wwiseRTPC.GetValue(soundManagerGameObject);
                    }
                }
            }
            else
            {
                Debug.LogError("SoundManager Error: Scriptable object or RTPC is null!");
            }
        }
        #endregion

        #region Callbacks
        private void Callback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
        {
            EventCallbackMsg.type = in_type;
            EventCallbackMsg.info = in_info;

            for (var i = 0; i < callbacks.Count; ++i)
                callbacks[i].CallFunction(EventCallbackMsg);
        }

        [System.Serializable]
        public class CallbackData
        {
            public AK.Wwise.CallbackFlags Flags;
            public string FunctionName;
            public GameObject GameObject;

            public void CallFunction(AkEventCallbackMsg eventCallbackMsg)
            {
                if (((uint)eventCallbackMsg.type & Flags.value) != 0 && GameObject)
                    GameObject.SendMessage(FunctionName, eventCallbackMsg);
            }
        }
        #endregion

        public IEnumerator WaitForDelay()
        {
            yield return new WaitForSeconds(triggerDelay);
            SelectBehavior();
        }
    }
    #endregion

    #region SoundEventListener
    public abstract class SoundEventListener<T, E, SE> : 
        IGameEventListener<T> where E : BaseGameEvent<T> where SE : SoundEvent
    {
        [SerializeField] private E gameEvent;
        [SerializeField] private SE soundEventClass;

        public E GameEvent
        {
            get { return gameEvent; }
            set { gameEvent = value; }
        }

        public SE SoundEventClass
        {
            get { return soundEventClass; }
            set { soundEventClass = value; }
        }

        public void Enable()
        {
            if (gameEvent == null)
            {
                return;
            }

            GameEvent.RegisterListener(this);
        }

        public void Disable()
        {
            if (gameEvent == null)
            {
                return;
            }

            GameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(T item)
        {
            if (item.GetType() == typeof(Void))
            {
                SoundEventClass.EventRaised(0);
            }
            else if (item.GetType() == typeof(float))
            {
                float tempFloat = float.Parse(item.ToString());
                Debug.Log(tempFloat);
                SoundEventClass.EventRaised(tempFloat);
            }

        }
    }

    public class SoundVoidEventListener : SoundEventListener<Void, VoidEvent, SoundEvent>
    {
    }

    public class SoundFloatEventListener : SoundEventListener<float, FloatEvent, SoundEvent>
    {
    }
    #endregion

    #region CustomEditor
#if UNITY_EDITOR
    [CustomEditor(typeof(SoundManager))]
    public class SoundManager_Editor : Editor
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
            DrawDefaultInspector(); // for other non-HideInInspector fields

            var script = target as SoundManager;

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
                            script.soundEvents[i].useOtherGameObject = EditorGUILayout.Toggle("Use other GameObj?",
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

                            if ((int) script.soundEvents[i].behaviorSelector == 4)
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

                            script.soundEvents[i].useOtherGameObject = EditorGUILayout.Toggle("Use other GameObj?",
                                script.soundEvents[i].useOtherGameObject);

                            if (script.soundEvents[i].useOtherGameObject)
                            {
                                SerializedProperty targetGameObjectProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].targetGameObject");
                                EditorGUILayout.PropertyField(targetGameObjectProp);
                            }

                            script.soundEvents[i].rtpcGlobal = EditorGUILayout.Toggle("Use Global RTPC", script.soundEvents[i].rtpcGlobal);

                            EditorGUILayout.Space();

                            EditorGUILayout.HelpBox("TRUE: Value controls RTPC  /  FALSE: RTPC controls value", MessageType.None);
                            script.soundEvents[i].fromScriptableObjToRtpc =
                                EditorGUILayout.Toggle("Set RTPC role", script.soundEvents[i].fromScriptableObjToRtpc);

                            script.soundEvents[i].setCustomRtpcFloat =
                                EditorGUILayout.Toggle("Use custom value?",
                                    script.soundEvents[i].setCustomRtpcFloat);

                            if (script.soundEvents[i].setCustomRtpcFloat)
                            {
                                script.soundEvents[i].customValue =
                                    EditorGUILayout.FloatField("Float Value", script.soundEvents[i].customValue);
                            }

                            EditorGUILayout.Space();

                            SerializedProperty scriptableObjectProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].rtpcScriptableObject");
                            EditorGUILayout.PropertyField(scriptableObjectProp);

                            if (!script.soundEvents[i].setCustomRtpcFloat)
                            {
                                EditorGUILayout.HelpBox("Make sure to use 'float events' if you want a value parsed from the event. Otherwise you can also set a custom value.", MessageType.Info);
                            }
                        }
                    }
                    #endregion

                    DrawUILine(true);
                }

            EditorGUI.indentLevel = EditorGUI.indentLevel - 2;

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