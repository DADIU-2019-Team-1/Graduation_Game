using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Team1_GraduationGame.Events;
using UnityEngine;
using UnityEngine.Events;

// NOTE: This script requires Wwise to work //

namespace Team1_GraduationGame.Managers
{
    [RequireComponent(typeof(AkGameObj))]
    public class SoundManager : MonoBehaviour
    {
        public enum colliderTypes
        {
            None,
            Box,
            Capsule,
            Mesh,
            Sphere,
            Wheel
        }

        [HideInInspector] public colliderTypes attachCollider;

        public SoundEvent[] soundEvents;
        private string[] tagStrings;
        private bool _collisionActive, _useStart;

        private void Awake()
        {
            tagStrings = new string[soundEvents.Length];

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
                    else if ((int) soundEvents[i].triggerTypeSelector == 2 ||
                             (int) soundEvents[i].triggerTypeSelector == 3)
                    {
                        _collisionActive = true;
                        if (soundEvents[i].tag != null && soundEvents[i].checkForTag)
                        {
                            tagStrings[i] = soundEvents[i].tag;
                        }
                    }
                    else if ((int) soundEvents[i].triggerTypeSelector == 4)
                    {
                        _useStart = true;
                    }

                    soundEvents[i].thisSoundManager = this;
                    soundEvents[i].soundEventId = i;
                    soundEvents[i].soundManagerGameObject = gameObject;
                }
        }

        private void Start()
        {
            if (_useStart)
            {
                for (int i = 0; i < soundEvents.Length; i++)
                {
                    if ((int) soundEvents[i].triggerTypeSelector == 4)
                    {
                        soundEvents[i].EventRaised(0);
                    }
                }
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

        private void OnTriggerEnter(Collider col)
        {
            if (_collisionActive)
            {
                for (int i = 0; i < soundEvents.Length; i++)
                {
                    if ((int) soundEvents[i].triggerTypeSelector == 2)
                    {
                        if (!soundEvents[i].checkForTag)
                        {
                            soundEvents[i].EventRaised(0);
                        }
                        else if (col.tag == tagStrings[i])
                        {
                            soundEvents[i].EventRaised(0);
                        }
                    }
                }
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (_collisionActive)
            {
                for (int i = 0; i < soundEvents.Length; i++)
                {
                    if ((int)soundEvents[i].triggerTypeSelector == 3)
                    {
                        if (!soundEvents[i].checkForTag)
                        {
                            soundEvents[i].EventRaised(0);
                        }
                        else if (col.tag == tagStrings[i])
                        {
                            soundEvents[i].EventRaised(0);
                        }
                    }
                }
            }
        }

        public void AttachCollider(int enumIndex)
        {
            Debug.Log("Attaching " + attachCollider + " collider to " + gameObject.name);
            switch (enumIndex)
            {
                case 0:
                    break;
                case 1:
                    gameObject.AddComponent<BoxCollider>();
                    break;
                case 2:
                    gameObject.AddComponent<CapsuleCollider>();
                    break;
                case 3:
                    gameObject.AddComponent<MeshCollider>();
                    break;
                case 4:
                    gameObject.AddComponent<SphereCollider>();
                    break;
                case 5:
                    gameObject.AddComponent<WheelCollider>();
                    break;
                default:
                    break;
            }
        }

        public void ResetEventInstance()
        {
            soundEvents[soundEvents.Length] = null;
        }

        public void ExternalRaise()
        {
            for (int i = 0; i < soundEvents.Length; i++)
            {
                if ((int)soundEvents[i].triggerTypeSelector == 5)
                {
                    soundEvents[i].EventRaised(0);
                }
            }
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
            Start,
            ExternalRaise
        }
        [HideInInspector] public EventTypeEnum triggerTypeSelector;
        [HideInInspector] public SoundVoidEventListener soundEventListener;
        [HideInInspector] public SoundFloatEventListener soundFloatEventListener;
        [HideInInspector] public float triggerDelay = 0.0f;
        [HideInInspector] public VoidEvent triggerEvent;
        [HideInInspector] public FloatEvent triggerFloatEvent;
        [HideInInspector] public bool rtpcRoleBool;
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
        [HideInInspector] public GameObject soundManagerGameObject, targetGameObject, secondGameObject;

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
        [HideInInspector] public bool useCallbacks, useActionOnEvent, rtpcGlobal, runOnce, 
            useOtherGameObject, setCustomRtpcFloat, isActive, objDistanceToRtpc, checkForTag, useValueFromEvent;

        // Other:
        [HideInInspector] public string tag;
        #endregion

        #region Wwise play/stop events
        public void PlayWwiseEvent()
        {

            if (wwiseEvent != null && (int) behaviorSelector == 0)
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
            if (wwiseEvent != null && (int) behaviorSelector == 0)
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

        private void AmbientHandler()   // TODO - YYY
        {

        }

        private float GetDistanceBetweenObjects(GameObject from, GameObject to)
        {
            float distance = Vector3.Distance(from.transform.position, to.transform.position);
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
        private void RTPCHandler()  // TODO: Implement use value from event
        {
            if (rtpcScriptableObject != null && wwiseRTPC != null)
            {
                if (rtpcRoleBool && !objDistanceToRtpc)
                {
                    if (rtpcGlobal)
                    {
                        if (!useValueFromEvent)
                            wwiseRTPC.SetGlobalValue(rtpcScriptableObject.value);
                        else
                            wwiseRTPC.SetGlobalValue(_parsedValue);
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
                else if (!rtpcRoleBool && !objDistanceToRtpc)
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
                else if (objDistanceToRtpc)
                {
                    if (rtpcGlobal)
                    {
                        if (!useOtherGameObject)
                            wwiseRTPC.SetGlobalValue(GetDistanceBetweenObjects(soundManagerGameObject, secondGameObject));
                        else
                            wwiseRTPC.SetGlobalValue(GetDistanceBetweenObjects(targetGameObject, secondGameObject));
                    }
                    else
                    {
                        if (targetGameObject == null || !useOtherGameObject)
                        {
                            wwiseRTPC.SetValue(soundManagerGameObject,
                                GetDistanceBetweenObjects(soundManagerGameObject, secondGameObject));
                            if (targetGameObject == null && useOtherGameObject)
                                Debug.LogWarning("SoundManager: Target GameObject is not set! - Using default object instead");
                        }
                        else
                            wwiseRTPC.SetValue(targetGameObject, GetDistanceBetweenObjects(targetGameObject, secondGameObject));
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

}