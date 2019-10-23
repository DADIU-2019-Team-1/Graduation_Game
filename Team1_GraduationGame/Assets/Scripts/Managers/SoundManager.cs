using System.Collections;
using System.Collections.Generic;
using Team1_GraduationGame.Events;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
                    if ((int) soundEvents[i].eventTypeSelector == 0)
                    {
                        soundEvents[i].soundEventListener = new SoundVoidEventListener();
                        soundEvents[i].soundEventListener.GameEvent = soundEvents[i].triggerEvent;
                        soundEvents[i].soundEventListener.SoundEventClass = soundEvents[i];
                        soundEvents[i].soundEventListener.Enable();
                    } 
                    else if ((int) soundEvents[i].eventTypeSelector == 1)
                    {
                        soundEvents[i].soundFloatEventListener = new SoundFloatEventListener();
                        soundEvents[i].soundFloatEventListener.GameEvent = soundEvents[i].triggerFloatEvent;
                        soundEvents[i].soundFloatEventListener.SoundEventClass = soundEvents[i];
                        soundEvents[i].soundFloatEventListener.Enable();
                    }

                    soundEvents[i].thisSoundManager = this;
                    soundEvents[i].soundEventId = i;
                    soundEvents[i].SoundManagerGameObject = gameObject;
                }
        }

        public void StartCoroutine(int id)
        {
            if (soundEvents[id] != null)
                StartCoroutine(soundEvents[id].WaitForDelay());
        }

        public void PlayAll()
        {
            for (int i = 0; i < soundEvents.Length; i++)
            {
                if (soundEvents[i].wwiseEvent != null && (int)soundEvents[i].eventTypeSelector == 0)
                    soundEvents[i].PlayWwiseEvent();
            }
        }
        public void StopAll()
        {
            Debug.Log("Stopping All Sounds");
            for (int i = 0; i < soundEvents.Length; i++)
            {
                if (soundEvents[i].wwiseEvent != null && (int)soundEvents[i].eventTypeSelector == 0)
                    soundEvents[i].StopWwiseEvent();
            }
        }
    }

    #region Sound Event Container
    [System.Serializable]
    public class SoundEvent
    {
        [HideInInspector] public SoundManager thisSoundManager;
        [HideInInspector] public int soundEventId;

        // Event system:
        public enum EventTypeEnum
        {
            Void,
            Float
        }

        [HideInInspector] public EventTypeEnum eventTypeSelector;
        [HideInInspector] public SoundVoidEventListener soundEventListener;
        [HideInInspector] public SoundFloatEventListener soundFloatEventListener;
        [HideInInspector] public float triggerDelay = 0.0f;
        [HideInInspector] public VoidEvent triggerEvent;
        [HideInInspector] public FloatEvent triggerFloatEvent;

        // Behavior switching:
        public enum BehaviorEnum
        {
            Event,
            State,
            Switch,
            RTPC,
            Debugger
        }

        [HideInInspector] public BehaviorEnum behaviorSelector;

        // GameObjects:
        [HideInInspector] public GameObject SoundManagerGameObject;
        [HideInInspector] public GameObject targetGameObject;

        // Wwise:
        private AkEventCallbackMsg EventCallbackMsg = new AkEventCallbackMsg();
        [HideInInspector] public List<CallbackData> callbacks;
        [HideInInspector] public AK.Wwise.Event wwiseEvent = new AK.Wwise.Event();
        [HideInInspector] public AK.Wwise.State wwiseState;
        [HideInInspector] public AK.Wwise.Switch wwiseSwitch;
        [HideInInspector] public AK.Wwise.RTPC WwiseRTPC;
        [HideInInspector] public FloatVariable rtpcScriptableObject;

        // Bools:
        [HideInInspector] public bool useCallbacks = false;
        [HideInInspector] public bool useActionOnEvent = false;


        public void PlayWwiseEvent()
        {
            if (wwiseEvent != null && (int) eventTypeSelector == 0)
            {
                Debug.Log("Playing Wwise Event");
                if (targetGameObject == null)
                    wwiseEvent.Post(SoundManagerGameObject);
                else
                    wwiseEvent.Post(targetGameObject);
            }
        }

        public void StopWwiseEvent()
        {
            if (wwiseEvent != null && (int) eventTypeSelector == 0)
            {
                Debug.Log("Stopping Wwise Event");
                if (targetGameObject == null)
                    wwiseEvent.Stop(SoundManagerGameObject);
                else
                    wwiseEvent.Stop(targetGameObject);
            }
        }

        public void EventRaised()
        {
            if (triggerDelay <= 0)
                SelectBehavior();
            else if (triggerDelay > 0)
                thisSoundManager.StartCoroutine(soundEventId);
        }

        private void SelectBehavior()
        {
            switch ((int)behaviorSelector)
            {
                case 0:
                    if (useCallbacks)
                        for (int i = 0; i < callbacks.Count; i++)
                        {
                            wwiseEvent.Post(callbacks[i].GameObject, callbacks[i].Flags, Callback);
                        }
                    else
                    {
                        PlayWwiseEvent();
                    }
                    break;
                case 1:
                    wwiseState.SetValue();
                    break;
                case 2:
                    wwiseSwitch.SetValue(SoundManagerGameObject);
                    break;
                case 3:
                    //
                    break;
                case 4:
                    Debug.Log("SoundManager Responds!");
                    break;
                default:
                    break;
            }
        }

        private void Callback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
        {
            EventCallbackMsg.type = in_type;
            EventCallbackMsg.info = in_info;

            for (var i = 0; i < callbacks.Count; ++i)
                callbacks[i].CallFunction(EventCallbackMsg);
        }

        public IEnumerator WaitForDelay()
        {
            yield return new WaitForSeconds(triggerDelay);
            SelectBehavior();
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
                Debug.Log("Void event raised");
                SoundEventClass.EventRaised();
            }
            else if (item.GetType() == typeof(float))
            {
                Debug.Log("Float event raised with float value: " + item);
                SoundEventClass.EventRaised();
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
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Note: This requires Wwise to work. Also please make sure that there is only one sound manager per scene", MessageType.None);
            DrawDefaultInspector(); // for other non-HideInInspector fields

            var script = target as SoundManager;

            if (GUILayout.Button("Play All"))
            {
                script.PlayAll();
            }

            if (GUILayout.Button("Stop All"))
            {
                script.StopAll();
            }

            if (script.soundEvents != null)
                for (int i = 0; i < script.soundEvents.Length; i++)
                {
                    DrawUILine(true);

                    SerializedProperty triggerEventSelectorProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].eventTypeSelector");
                    EditorGUILayout.PropertyField(triggerEventSelectorProp);

                    if ((int) script.soundEvents[i].eventTypeSelector == 0)
                    {
                        SerializedProperty triggerEventProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].triggerEvent");
                        EditorGUILayout.PropertyField(triggerEventProp);
                    }
                    else if ((int) script.soundEvents[i].eventTypeSelector == 1)
                    {
                        SerializedProperty triggerFloatEventProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].triggerFloatEvent");
                        EditorGUILayout.PropertyField(triggerFloatEventProp);
                    }

                    script.soundEvents[i].triggerDelay =
                        EditorGUILayout.FloatField("Trigger Delay", script.soundEvents[i].triggerDelay);

                    SerializedProperty behaviorSelectorProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].behaviorSelector");
                    EditorGUILayout.PropertyField(behaviorSelectorProp);

                    DrawUILine(false);

                    if ((int) script.soundEvents[i].behaviorSelector == 0)
                    {
                        SerializedProperty targetGameObjectProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].targetGameObject");
                        EditorGUILayout.PropertyField(targetGameObjectProp);

                        SerializedProperty eventDataProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].wwiseEvent");
                        EditorGUILayout.PropertyField(eventDataProp);

                        script.soundEvents[i].useActionOnEvent = EditorGUILayout.Toggle("Do Action On Event?", script.soundEvents[i].useActionOnEvent);
                        script.soundEvents[i].useCallbacks = EditorGUILayout.Toggle("Use Callbacks?", script.soundEvents[i].useCallbacks);

                        if (script.soundEvents[i].useActionOnEvent)
                        {
                            DrawUILine(false);
                            // TODO
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
                    else if ((int) script.soundEvents[i].behaviorSelector == 1)
                    {
                        SerializedProperty stateProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].wwiseState");
                        EditorGUILayout.PropertyField(stateProp);
                    }
                    else if ((int) script.soundEvents[i].behaviorSelector == 2)
                    {
                        SerializedProperty switchProp = serializedObject.FindProperty("soundEvents.Array.data[" + i + "].wwiseSwitch");
                        EditorGUILayout.PropertyField(switchProp);
                    }

                }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
            }

        }

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
    }
#endif
    #endregion
}