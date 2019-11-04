using Team1_GraduationGame.Enemies;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Team1_GraduationGame.Interaction
{
    public class Interactable : MonoBehaviour
    {
        // References:
        private Movement _movement;
        public Enemy thisEnemy;
        private GameObject _player;
        private Animator _animator;
        private ObjectPush _objectPush;
        public FloatVariable interactionDistance;
        public FloatVariable interactionAngle;

        // Public:
        public float minDistance = 2.0f, angle = 75.0f;
        public bool interactableOnce, pushable, useEvents, useAnimation, switchBetweenAnimations, animationState, 
            interactConditions = true, checkForObstructions;
        public UnityEvent eventOnInteraction;
        public string animationDefault, animationAction;

        // Private:
        private bool _isEnemy, _interacted;
        private int _layerMask;


        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _layerMask = ~LayerMask.GetMask("Enemies");


            if (_player != null)
                if (_player.GetComponent<Movement>() != null)
                    _movement = _player.GetComponent<Movement>();

            if (GetComponent<Enemy>() != null)
            {
                _isEnemy = true;
                thisEnemy = GetComponent<Enemy>();
            }

            if (GetComponent<Animator>() != null)
            {
                _animator = GetComponent<Animator>();
            }

            if (GetComponent<ObjectPush>() != null)
            {
                _objectPush = GetComponent<ObjectPush>();
            }

            if (interactionDistance != null)
            {
                minDistance = interactionDistance.value;
            }

            if (interactionAngle != null)
            {
                angle = interactionAngle.value;
            }
        }

        public void Interact()
        {
            if (!_interacted)
            {

                if (!interactConditions)
                {
                    DoAction();
                }
                else
                {
                    Vector3 dir = _player.transform.position - transform.position;
                    float thisToPlayerAngle = Vector3.Angle(transform.forward, dir);
                    RaycastHit hit;

                    if (pushable && thisEnemy == null && checkForObstructions)
                    {
                        if (Vector3.Distance(transform.position, _player.transform.position) < minDistance 
                            && thisToPlayerAngle < angle / 2 && Physics.Raycast(transform.position + transform.up, dir, out hit, minDistance, _layerMask))
                        {   // TODO Make this work the other direction also
                            DoAction();
                        }
                    }
                    else if (pushable && thisEnemy == null && !checkForObstructions)
                    {
                        if (Vector3.Distance(transform.position, _player.transform.position) < minDistance
                            && thisToPlayerAngle < angle / 2)   // TODO Make this work the other direction also
                        {
                            DoAction();
                        }
                    }
                    else if (checkForObstructions)
                    {
                        if (Vector3.Distance(transform.position, _player.transform.position) < minDistance
                            && Physics.Raycast(transform.position + transform.up, dir, out hit, minDistance,
                                _layerMask))
                        {
                            DoAction();
                        }
                    }
                    else if (!checkForObstructions)
                    {
                        if (Vector3.Distance(transform.position, _player.transform.position) < minDistance)
                        {
                            DoAction();
                        }
                    }

                }
                
            }
        }

        private void DoAction()
        {
            if (useEvents)
                eventOnInteraction.Invoke();

            if (pushable)
            {
                if (_isEnemy)
                    thisEnemy.PushDown();
                else if (_objectPush != null && interactConditions)
                {
                    _objectPush.Push(false);
                }
                else if (!interactConditions && _objectPush != null)
                {
                    _objectPush.Push(true);
                }
            }

            if (useAnimation && _animator != null)
            {
                if (switchBetweenAnimations)
                {
                    _animator.Play(animationState ? animationDefault : animationAction);
                    animationState = !animationState;
                }
                else
                {
                    _animator.Play(animationAction);
                }
            }
            else if (useAnimation && _animator == null)
            {
                Debug.LogError("Interaction Error: Animator missing on " + gameObject.name);
            }

            if (interactableOnce)
                _interacted = true;
        }

    }

    #region Custom Inspector
#if UNITY_EDITOR
    [CustomEditor(typeof(Interactable))]
    public class Interactable_Inspector : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {

            // DrawDefaultInspector();

            var script = target as Interactable;

            if (script.gameObject.GetComponent<Enemy>() != null)
            {
                if (script.thisEnemy == null)
                {
                    script.thisEnemy = script.gameObject.GetComponent<Enemy>();
                }
            }

            // Bools:
            script.interactableOnce = EditorGUILayout.Toggle("Run Once?", script.interactableOnce);

            // Conditions:
            DrawUILine(false);
            script.interactConditions = EditorGUILayout.Toggle("Interaction Conditions?", script.interactConditions);

            if (script.interactConditions)
            {
                script.checkForObstructions =
                    EditorGUILayout.Toggle("Check for obstructions?", script.checkForObstructions);

                if (script.interactionDistance == null)
                {
                    script.minDistance = EditorGUILayout.FloatField("Distance", script.minDistance);
                }

                SerializedProperty distanceProp = serializedObject.FindProperty("interactionDistance");
                EditorGUILayout.PropertyField(distanceProp);

                if (script.thisEnemy == null && script.pushable)
                {
                    if (script.interactionAngle == null)
                    {
                        script.angle = EditorGUILayout.FloatField("Angle", script.angle);
                    }

                    SerializedProperty angleProp = serializedObject.FindProperty("interactionAngle");
                    EditorGUILayout.PropertyField(angleProp);
                }
            }

            // Pushable:
            DrawUILine(false);
            script.pushable = EditorGUILayout.Toggle("Pushable?", script.pushable);

            if (script.pushable)
            {
                if (script.gameObject.GetComponent<ObjectPush>() == null && script.GetComponent<Enemy>() == null)
                {
                    script.gameObject.AddComponent<ObjectPush>();
                }
            }

            // Events:
            DrawUILine(false);
            script.useEvents = EditorGUILayout.Toggle("Use Events?", script.useEvents);

            if (script.useEvents)
            {
                SerializedProperty eventProp = serializedObject.FindProperty("eventOnInteraction");
                EditorGUILayout.PropertyField(eventProp);
                DrawUILine(false);
            }

            // Animation
            DrawUILine(false);
            script.useAnimation = EditorGUILayout.Toggle("Use Animation?", script.useAnimation);

            if (script.useAnimation)
            {
                script.switchBetweenAnimations = EditorGUILayout.Toggle("Switch between animations?", script.switchBetweenAnimations);
                if (script.switchBetweenAnimations)
                    script.animationDefault = EditorGUILayout.TextField("Animation Default", script.animationDefault);

                script.animationAction = EditorGUILayout.TextField("Animation Action", script.animationAction);
                DrawUILine(false);
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
                thickness = 4;
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