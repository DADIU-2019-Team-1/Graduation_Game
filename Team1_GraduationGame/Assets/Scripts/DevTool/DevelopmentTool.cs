using Team1_GraduationGame.Enemies;

namespace Team1_GraduationGame.DevelopmentTools
{
    using System.Collections;
    using System.Collections.Generic;
    using Team1_GraduationGame.SaveLoadSystem;
    using UnityEngine;
    using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class DevelopmentTool : MonoBehaviour
    {
        // Public:
        public SavePointManager thisSavePointManager;

        // UI:
        [HideInInspector] public InputField goToSavePointNum;
        [HideInInspector] public InputField goToLevelNum;
        [HideInInspector] public Text warningText;
        [HideInInspector] public GameObject mainPanel;
        [HideInInspector] public Text debugText;
        [HideInInspector] public Text fpsText;

        // Bools:
        public bool setUpEnable = false;

        // Private:
        string _dLog;
        Queue _dLogQueue = new Queue();

        private void Awake()
        {
            if (thisSavePointManager == null)
            {
                thisSavePointManager = GameObject.FindObjectOfType<SavePointManager>();
            }

            if (thisSavePointManager == null && warningText != null)
            {
                warningText.gameObject.SetActive(true);
            }

            Application.logMessageReceived += Log;
        }

        void OnEnable()
        {
            Application.logMessageReceived += Log;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= Log;
        }

        void Log(string logString, string stackTrace, LogType type)
        {
            _dLog = logString;
            string newString = "\n [" + type + "] : " + _dLog;
            _dLogQueue.Enqueue(newString);
            if (type == LogType.Exception)
            {
                newString = "\n" + stackTrace;
                _dLogQueue.Enqueue(newString);
            }
            _dLog = string.Empty;
            foreach (string dLog in _dLogQueue)
            {
                _dLog += dLog;
            }
        }

        private void Update()
        {
            if (debugText != null)
                debugText.text = _dLog;

            if (fpsText != null)
                fpsText.text = (1.0f / Time.deltaTime).ToString();

        }

        /// <summary>
        /// This sets the Debug/Developer Tools panel active/inactive.
        /// </summary>
        /// <param name="active">False: panel disabled, True: panel enabled</param>
        public void SetDevelopmentToolPanel(bool active)
        {
            if (active && mainPanel != null)
            {
                mainPanel.SetActive(true);
            }
            else if (mainPanel != null)
            {
                mainPanel.SetActive(false);
            }
        }

        public void TeleportToSavePoint()
        {
            if (goToSavePointNum != null)
            {
                thisSavePointManager?.TeleportToSavePoint(int.Parse(goToSavePointNum.text));
            }
        }

        public void goToLevel()
        {
            if (goToLevelNum != null)
                new SaveLoadManager().OpenLevel(int.Parse(goToLevelNum.text));
        }

        public void DisableSaving()
        {
            thisSavePointManager?.DisableSavingOnSavePoints();
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DevelopmentTool))]
    public class DevelopmentTool_Editor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // for other non-HideInInspector fields

            var script = target as DevelopmentTool;

            if (script.setUpEnable)
            {
                DrawUILine();

                SerializedProperty goToSavePointNumProp = serializedObject.FindProperty("goToSavePointNum");
                EditorGUILayout.PropertyField(goToSavePointNumProp);

                SerializedProperty goToLevelNumProp = serializedObject.FindProperty("goToLevelNum");
                EditorGUILayout.PropertyField(goToLevelNumProp);

                SerializedProperty warningTextProp = serializedObject.FindProperty("warningText");
                EditorGUILayout.PropertyField(warningTextProp);

                SerializedProperty mainPanelProp = serializedObject.FindProperty("mainPanel");
                EditorGUILayout.PropertyField(mainPanelProp);

                SerializedProperty debugTextProp = serializedObject.FindProperty("debugText");
                EditorGUILayout.PropertyField(debugTextProp);

                SerializedProperty fpsTextProp = serializedObject.FindProperty("fpsText");
                EditorGUILayout.PropertyField(fpsTextProp);
            }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
            }
        }

        #region DrawUILine function
        public static void DrawUILine()
        {
            Color color = new Color(1, 1, 1, 0.3f);
            int thickness = 1;
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