using System.Collections;
using Team1_GraduationGame.Enemies;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Team1_GraduationGame.Interaction
{
    public class Interactable : MonoBehaviour
    {
        // References:
        public Enemy thisEnemy;
        private Enemy[] _allEnemies;
        private GameObject _player;
        private Animator _animator;
        private ObjectPush _objectPush;
        public FloatVariable interactionDistance;
        public FloatVariable interactionAngle;

        // Public:
        public float minDistance = 2.0f, angle = 90.0f, soundEmitDistance, interactCooldown = 1.5f;
        public bool interactableOnce, pushable, useEvents, useAnimation, switchBetweenAnimations, animationState, 
            interactConditions = true, checkForObstructions, emitSound, useCooldown;
        public UnityEvent eventOnInteraction;
        public string animationDefault, animationAction;
        [HideInInspector] public bool toggleState;

        // Private:
        private bool _isEnemy, _interacted;
        private int _layerMask;

        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _layerMask = ~LayerMask.GetMask("Enemies");

            if (GetComponent<Enemy>() != null)
            {
                _isEnemy = true;
                emitSound = false;
                thisEnemy = GetComponent<Enemy>();
            }
            else
                _allEnemies = FindObjectsOfType<Enemy>();
            

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
                    DoAction(0);
                }
                else if (_objectPush.wayPoints.Count <= 2)
                {
                    Vector3 dir = _player.transform.position - transform.position;
                    float thisToPlayerAngle1 = Vector3.Angle(_objectPush.wayPoints[0].transform.position - _objectPush.wayPoints[1].transform.position, dir);
                    float thisToPlayerAngle2 = Vector3.Angle(_objectPush.wayPoints[1].transform.position - _objectPush.wayPoints[0].transform.position, dir);
                    RaycastHit hit;

                    if (pushable && thisEnemy == null)
                    {
                        if (checkForObstructions)
                        {
                            if (Vector3.Distance(transform.position, _player.transform.position) < minDistance 
                                && Physics.Raycast(transform.position + transform.up, dir, out hit, minDistance, _layerMask))
                            {
                                if (thisToPlayerAngle1 < angle / 2)
                                    DoAction(2);
                                else if (thisToPlayerAngle2 < angle / 2)
                                    DoAction(1);
                            }
                        }
                        else
                        {
                            if (Vector3.Distance(transform.position, _player.transform.position) < minDistance)
                            {
                                if (thisToPlayerAngle1 < angle / 2)
                                    DoAction(2);
                                else if (thisToPlayerAngle2 < angle / 2)
                                    DoAction(1);
                            }
                        }
                    }
                    else if (thisEnemy != null)
                    {
                        if (checkForObstructions)
                        {
                            if (Vector3.Distance(transform.position, _player.transform.position) < minDistance
                                && Physics.Raycast(transform.position + transform.up, dir, out hit, minDistance,
                                    _layerMask))
                            {
                                DoAction(0);
                            }
                        }
                        else
                        {
                            if (Vector3.Distance(transform.position, _player.transform.position) < minDistance)
                            {
                                DoAction(0);
                            }
                        }
                    }
                }

                if (useCooldown && !interactableOnce)
                {
                    StartCoroutine(InteractionCooldown());
                }
            }
        }

        /// <summary>
        /// Do interaction action - Must be called from Interact()
        /// </summary>
        /// <param name="dir">0 = No direction, 1 = forward, 2 = backwards</param>
        private void DoAction(int dir)
        {

            if (useEvents)
                eventOnInteraction.Invoke();

            if (pushable)
            {
                if (_isEnemy)
                    thisEnemy.PushDown();
                else if (_objectPush != null && interactConditions)
                {
                    _objectPush.Push(false, dir);
                }
                else if (!interactConditions && _objectPush != null)
                {
                    _objectPush.Push(true, dir);
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

            if (emitSound)
                HearingCheck();

            if (interactableOnce)
                _interacted = true;
        }

        private void HearingCheck()
        {
            for (int i = 0; i < _allEnemies.Length; i++)
            {
                if (!_allEnemies[i].GetHearing() && !_allEnemies[i].alwaysAggro)
                {
                    NavMeshPath path = new NavMeshPath();
                    _allEnemies[i].getNavMeshAgent().CalculatePath(transform.position, path);

                    Vector3[] allPathPoints = new Vector3[path.corners.Length + 2];
                    allPathPoints[0] = _allEnemies[i].gameObject.transform.position;
                    allPathPoints[allPathPoints.Length - 1] = transform.position;

                    for (int j = 0; j < path.corners.Length; j++)
                    {
                        allPathPoints[j + 1] = path.corners[j];
                    }

                    float tempPathLength = 0.0f;

                    for (int j = 0; j < allPathPoints.Length - 1; j++)
                    {
                        tempPathLength += Vector3.Distance(allPathPoints[j], allPathPoints[j + 1]);
                    }

                    if (tempPathLength < soundEmitDistance && soundEmitDistance > 0.0f)
                    {
                        _allEnemies[i].SetLastSighting(_player.transform.position);
                        _allEnemies[i].SetAggro(true);
                    }
                }
            }
        }

        private IEnumerator InteractionCooldown()
        {
            _interacted = true;
            yield return new WaitForSeconds(interactCooldown);
            _interacted = false;
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

            if (script.gameObject.GetComponent<Enemy>() == null && script.GetComponent<NavMeshObstacle>() == null)
            {
                if (script.gameObject.tag != "Interactable")
                    script.gameObject.tag = "Interactable";
                NavMeshObstacle tempNavMeshObs = script.gameObject.AddComponent<NavMeshObstacle>();
                tempNavMeshObs.carving = true;
                tempNavMeshObs.carveOnlyStationary = false;
            }

            // Bools:
            script.interactableOnce = EditorGUILayout.Toggle("Run Once?", script.interactableOnce);
            if (!script.interactableOnce)
            {
                script.useCooldown = EditorGUILayout.Toggle("Use Cooldown?", script.useCooldown);
                if (script.useCooldown)
                    script.interactCooldown = EditorGUILayout.FloatField("Time (sec)", script.interactCooldown);
            }

            // Sound emit:
            DrawUILine(false);
            if (script.thisEnemy == null)
            {
                EditorGUILayout.HelpBox("Use the 'sound manager' script if you want to play actual sounds", MessageType.None);
                script.emitSound = EditorGUILayout.Toggle("Emit Sound on Use?", script.emitSound);

                if (script.emitSound)
                    script.soundEmitDistance = EditorGUILayout.FloatField("Sound Distance", script.soundEmitDistance);
            }

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