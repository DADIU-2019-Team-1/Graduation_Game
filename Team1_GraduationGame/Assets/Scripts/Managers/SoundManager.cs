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

        private void Start()
        {
            if (soundEvents != null)
                for (int i = 0; i < soundEvents.Length; i++)
                {
                    soundEvents[i].soundEventListener.SoundEventClass = soundEvents[i];
                }
        }

        private void Update()
        {
            // soundEvents[0].soundEventTrigger.RegisterListener(soundEvents[0].soundEventListener);
        }

    }

    public abstract class SoundEventListener<T, E, SE> : MonoBehaviour,
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

        private void OnEnable()
        {
            if (gameEvent == null)
                return;

            GameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (gameEvent == null)
                return;

            GameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(T item)
        {
            SoundEventClass.DebugLog();
        }
    }

    public class SoundVoidEventListener : SoundEventListener<Void, VoidEvent, SoundEvent>
    {
    }

    [System.Serializable]
    public class SoundEvent
    {
        public string debugString;
        public VoidEvent soundEventTrigger;

        //public RTPC_SO RTPC_ScriptableObject;

        public SoundVoidEventListener soundEventListener;

        public void PlayAllSounds() // TODO
        {
            Debug.Log("Playing All Sounds");
            for (int i = 0; i < 1; i++)
            {
                DebugLog(); // Only for debugging
                // TODO: iterate though all attached, somehow
            }
        }

        public void DebugLog()
        {
            Debug.Log(debugString);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SoundManager))]
    public class SoundManager_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Note: This requires Wwise to work. Also please make sure that there is only one sound manager per scene", MessageType.None);
            DrawDefaultInspector(); // for other non-HideInInspector fields

            var script = target as SoundManager;

            if (script.soundEvents != null)
                for (int i = 0; i < script.soundEvents.Length; i++)
                {
                    if (GUILayout.Button("Play All"))
                    {
                        script.soundEvents[i].PlayAllSounds();
                    }

                }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
            }
        }
    }
#endif

}