using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.XR;

namespace Team1_GraduationGame.MotionMatching
{
    [RequireComponent(typeof(Animator))]
    public class MotionMatching : MonoBehaviour
    {
        // TODOs
        // Must have
        // TODO: Revise pose matching
        // TODO: Cut down idle animations (lots of repetition)
        // TODO: Store animations in seperate "state" lists for performance
        // TODO: Convert system to Unity DOTS - can only take NativeArrays<float3> *****

        // Should have
        // TODO: Simpler comparison and weight system
        // TODO: Extrapolate empty trajectorypoints (points that go over the frame size for that clip)
        // TODO: Collision detection with raycasting between the trajectory points

        // Nice to have
        // TODO: Create LookUp system in preproccesing, that can be used instead of pose matching during runtime
        // TODO: Create some debugger that shows various information about the data, especially the trajectory for each frame
        // TODO: When preprocessing, also store the data that is being written to CSV as return to feature vector (do load and write step together when preprocessing)
        // TODO: Use idle threshold reference from Movement
        // TODO: Add idle events
        // TODO: Add method summaries and general documentation 


        // --- References
        private Movement movement;
        private PreProcessing preProcessing;
        private Animator animator;

        // --- Collections
        private List<FeatureVector> featureVectors, _trajCandidatesRef, _trajPossibleCandidatesRef;
        private List<float> _trajCandidateValuesRef;
        private AnimationClip[] allClips;
        public HumanBodyBones[] joints;
        public AnimContainer animContainer; // put ref to chosen animation container scriptable object
        [SerializeField] private Queue<int> banQueue;
        [SerializeField] private string[] states;

        // --- Variables 
        public bool _preProcess, _playAnimationMode;
        public int pointsPerTrajectory = 4, framesBetweenTrajectoryPoints = 10;
        [SerializeField] private bool _isMotionMatching;
        [SerializeField] private int queryRateInFrames = 10, candidatesPerMisc = 10, banQueueSize = 10;

        [Tooltip("The time in frames that it takes for the animation layers to blend.")] 
        [SerializeField] private float animLayerWeightChange = 5.0f;

        private AnimationClip currentClip;
        private int currentFrame, _currentID;
        [SerializeField] private float animationFrameRate = 50.0f;

        // --- Weights
        [Range(0, 1)] public float weightRootVel = 1.0f,
            weightLFootVel = 1.0f,
            weightRFootVel = 1.0f,
            weightNeckVel = 1.0f,
            weightNeckPos = 1.0f,
            weightFeetPos = 1.0f,
            weightTrajPositions = 1.0f,
            weightTrajForwards = 1.0f;

        // --- Debugstuff
        [Header("Debug")] [SerializeField] private bool useJobs = true;
        private int animIterator = -1;
        private IEnumerator currentEnumerator;
        private List<string> debugStringList;
        
        private NativeArray<float3> _trajectoryPositions, _trajectoryForwards;
        private NativeArray<int> _featureIDs, _featureFrame, _frameCountForIDs, _featureState;

        private void Awake() // Load animation data
        {
            movement = GetComponent<Movement>();
            animator = GetComponent<Animator>();
            preProcessing = new PreProcessing();

            for (int i = 0; i < states.Length; i++)
            {
                states[i] = states[i].ToLower();
            }

#if UNITY_EDITOR
            if (_preProcess)
            {
                // Get animations from animation controller, and store it in a scriptable object
                if (animator != null)
                {
                    allClips = animator.runtimeAnimatorController.animationClips;
                    if (allClips == null)
                        Debug.LogError(
                            "Durnig preprocessing, tried to find animation clips in the animator controller, but there was none!");
                }
                else
                {
                    Debug.LogError("No Animator was found in the supplied GameObject during mm preprocessing!",
                        gameObject);
                }

                AnimContainer tempAnimContainer = new AnimContainer();
                tempAnimContainer.animationClips = allClips;
                EditorUtility.CopySerialized(tempAnimContainer, animContainer);
                AssetDatabase.SaveAssets();

                preProcessing.Preprocess(allClips, joints, gameObject, animator, animationFrameRate, states);
            }
#endif
            allClips = animContainer.animationClips;
            featureVectors = preProcessing.LoadData(pointsPerTrajectory, framesBetweenTrajectoryPoints);

            for (int i = 0; i < allClips.Length; i++)
            {
                int frames = (int) (allClips[i].length * allClips[i].frameRate);
                for (int j = 0; j < featureVectors.Count; j++)
                {
                    if (featureVectors[j].GetClipName() == allClips[i].name)
                        featureVectors[j].SetFrameCount(frames);
                }
            }


            _trajCandidatesRef = new List<FeatureVector>();
            _trajPossibleCandidatesRef = new List<FeatureVector>();
            _trajCandidateValuesRef = new List<float>();
            debugStringList = new List<string>();
            banQueue = new Queue<int>();
        }

        private void Start()
        {
            // Persistent native array initialization
            _trajectoryPositions = new NativeArray<float3>(featureVectors.Count * pointsPerTrajectory, Allocator.Persistent);
            _trajectoryForwards = new NativeArray<float3>(featureVectors.Count * pointsPerTrajectory, Allocator.Persistent);
            _featureIDs = new NativeArray<int>(featureVectors.Count, Allocator.Persistent);
            _featureFrame = new NativeArray<int>(featureVectors.Count, Allocator.Persistent);
            _frameCountForIDs = new NativeArray<int>(featureVectors.Count, Allocator.Persistent);
            _featureState = new NativeArray<int>(featureVectors.Count, Allocator.Persistent);

            for (int i = 0; i < featureVectors.Count; i++)
            {
                _featureIDs[i] = featureVectors[i].GetID();
                _featureFrame[i] = featureVectors[i].GetFrame();
                _frameCountForIDs[i] = featureVectors[i].GetFrameCountForID();
                for (int j = 0; j < pointsPerTrajectory; j++)
                {
                    _trajectoryPositions[i + j] = featureVectors[i].GetTrajectory().GetTrajectoryPoints()[j].GetPoint();
                    _trajectoryForwards[i + j] = featureVectors[i].GetTrajectory().GetTrajectoryPoints()[j].GetForward();
                }

                _featureState[i] = featureVectors[i].GetState();
            }

            if (!_playAnimationMode)
            {


                UpdateAnimation(0, 0);
                StartCoroutine(MotionMatch());
            }
        }


        public void ChangeLayerWeight(int layerIndex, int desiredWeight)
        {
            StopCoroutine(nameof(ChangeLayerWeightOverTime));
            StartCoroutine(ChangeLayerWeightOverTime(layerIndex, desiredWeight));
        }

        private IEnumerator ChangeLayerWeightOverTime(int layerIndex, int desiredWeight)
        {
            float counter = 0.0f;
            float layerWeight = animator.GetLayerWeight(layerIndex);
            while (counter <= animLayerWeightChange)
            {
                animator.SetLayerWeight(layerIndex, math.lerp(layerWeight, desiredWeight, counter / animLayerWeightChange));
                yield return new WaitForSeconds(Time.fixedDeltaTime);
                counter++;
            }
        }


#if UNITY_EDITOR
        private void Update()
        {
            if (_playAnimationMode)
            {
                StopCoroutine(MotionMatch());
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (currentEnumerator != null)
                        StopCoroutine(currentEnumerator);
                    transform.position = Vector3.zero;
                    StartCoroutine(PlayAnimation());
                }
            }
        }
#endif

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Matrix4x4 invCharSpace = transform.worldToLocalMatrix.inverse;
                Matrix4x4 animSpace = new Matrix4x4();
                animSpace.SetTRS(featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[0].GetPoint(),
                    Quaternion.identity, Vector3.one);

                Gizmos.color = Color.red; // Movement Trajectory
                for (int i = 0;
                    i < movement.GetMovementTrajectory().GetTrajectoryPoints().Length;
                    i++) // Gizmos for movement
                {
                    // Position
                    //Gizmos.DrawWireSphere(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(), 0.2f);
                    if (i != 0)
                    {
                        Gizmos.DrawLine(movement.GetMovementTrajectory().GetTrajectoryPoints()[i - 1].GetPoint(), movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint());
                    }
                    else
                    {
                        Gizmos.DrawLine(transform.position, movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint());
                    }

                    // Forward
                    Gizmos.DrawLine(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(),
                        movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint() +
                        movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetForward());
                }

                Gizmos.color = Color.green; // Animation Trajectory
                for (int i = 0; i < featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints().Length; i++)
                {
                    // Position
                    //Gizmos.DrawWireSphere(invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint())), 0.2f);

                    if (i != 0)
                    {
                        Gizmos.DrawLine(
                            invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(
                                featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i - 1].GetPoint())),
                            invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(
                                featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint())));
                    }

                    // Forward
                    Gizmos.DrawLine(
                        invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(
                            featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint())),
                        invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(
                            featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint())) +
                        invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(
                            featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetForward())));
                }
            }
        }

        private void UpdateAnimation(int id, int frame)
        {
            for (int i = 0; i < allClips.Length; i++)
            {
                if (allClips[i].name == featureVectors[id].GetClipName())
                {
                    currentClip = allClips[i];
                    break;
                }
            }

            //Debug.Log("Updating to animation " + currentClip.name + " to frame " + frame + " with ID " + id + " from id " + _currentID + " of frame " + currentFrame);
            animator.CrossFadeInFixedTime(currentClip.name, 0.3f, 0,
                frame / animationFrameRate); // 0.3f was recommended by Magnus
            _currentID = id;
            currentFrame = frame;
            banQueue.Enqueue(_currentID);
            if (banQueue.Count > banQueueSize)
                banQueue.Dequeue();
        }

        private IEnumerator MotionMatch()
        {
            _isMotionMatching = true;
            while (_isMotionMatching)
            {
                _currentID += queryRateInFrames;
                List<FeatureVector> candidates = TrajectoryMatching(movement.GetMovementTrajectory(),
                    ref _trajCandidatesRef, ref _trajPossibleCandidatesRef, ref _trajCandidateValuesRef);
                int candidateID = PoseMatching(candidates);
                UpdateAnimation(candidateID, featureVectors[candidateID].GetFrame());
                yield return new WaitForSeconds(queryRateInFrames / animationFrameRate);
            }
        }

        private IEnumerator PlayAnimation()
        {
            animIterator++;
            if (animIterator == allClips.Length)
            {
                animIterator = 0;
                Debug.Log("Finished playing last animation!");
            }

            for (int i = 0; i < featureVectors.Count; i++)
            {
                if (featureVectors[i].GetClipName() == allClips[animIterator].name)
                {
                    _currentID = i;
                    break;
                }
            }

            int startofIdForClip = _currentID;
            Debug.Log("Current playing " + allClips[animIterator].name + ", which is " + allClips[animIterator].length +
                      " seconds long!");
            Debug.Log("While loop condition: Frame " + featureVectors[_currentID].GetFrame() + " < " +
                      featureVectors[_currentID].GetFrameCountForID() + " && Clip " +
                      featureVectors[_currentID].GetClipName() + " == " + allClips[animIterator].name);
            while (featureVectors[_currentID].GetFrame() < featureVectors[_currentID].GetFrameCountForID() &&
                   featureVectors[_currentID].GetClipName() == allClips[animIterator].name)
            {
                Debug.Log("Current ID is now " + _currentID + ", which started at ID " + startofIdForClip + "!");
                UpdateAnimation(_currentID, featureVectors[_currentID].GetFrame());
                yield return new WaitForSeconds(queryRateInFrames / currentClip.frameRate);
                _currentID += queryRateInFrames;
            }
        }

        List<FeatureVector> TrajectoryMatching(Trajectory movementTraj, ref List<FeatureVector> candidates, ref List<FeatureVector> possibleCandidates, ref List<float> candidateValues)
        {
            float startTime = Time.realtimeSinceStartup;
            candidates.Clear();
            if (useJobs)
            {
                NativeArray<int> trajectoryCandidateIDs = new NativeArray<int>(candidatesPerMisc, Allocator.TempJob);
                NativeArray<float> trajectoryCandidateValues = new NativeArray<float>(candidatesPerMisc, Allocator.TempJob);
                int movementTrajectoryLength = movementTraj.GetTrajectoryPoints().Length;
                NativeArray<float3> movementTrajectoryPositions = new NativeArray<float3>(movementTrajectoryLength, Allocator.TempJob);
                NativeArray<float3> movementTrajectoryForwards = new NativeArray<float3>(movementTrajectoryLength, Allocator.TempJob);

                for (int i = 0; i < movementTrajectoryLength; i++)
                {
                    movementTrajectoryPositions[i] = movementTraj.GetTrajectoryPoints()[i].GetPoint();
                    movementTrajectoryForwards[i] = movementTraj.GetTrajectoryPoints()[i].GetForward();
                }

                TrajectoryMatchingParallelJob parallelJob = new TrajectoryMatchingParallelJob
                {
                    animPositionArray = _trajectoryPositions,
                    animForwardArray = _trajectoryForwards,
                    movementPositionArray = movementTrajectoryPositions,
                    movementForwardArray = movementTrajectoryForwards,
                    featureIDs = _featureIDs,
                    featureState = _featureState,
                    featureFrame = _featureFrame,
                    frameCountForIDs = _frameCountForIDs,
                    candidateIDs = trajectoryCandidateIDs,
                    candidateValues = trajectoryCandidateValues,
                    currentID = _currentID,
                    queryRate = queryRateInFrames,
                    state = movement.moveState.value,
                    trajPoints = pointsPerTrajectory,
                    movementMatrix = transform.localToWorldMatrix.inverse,
                    positionWeight = weightTrajPositions,
                    forwardWeight = weightTrajForwards
                };
                JobHandle handle = parallelJob.Schedule();
                handle.Complete();

                for (int i = 0; i < trajectoryCandidateIDs.Length; i++)
                {
                    candidates.Add(featureVectors[trajectoryCandidateIDs[i]]);
                }
                trajectoryCandidateIDs.Dispose();
                trajectoryCandidateValues.Dispose();
                movementTrajectoryPositions.Dispose();
                movementTrajectoryForwards.Dispose();
            }
            else
            {
                possibleCandidates.Clear();
                candidateValues.Clear();
                for (int i = 0; i < candidatesPerMisc; i++)
                {
                    possibleCandidates.Add(null);
                    candidateValues.Add(float.MaxValue);
                }

                for (int i = 0; i < featureVectors.Count; i++)
                {
                    if (!AnimDiscarder(featureVectors[i].GetClipName(), movement.moveState.value))
                    {
                        continue;
                    }

                    if ((featureVectors[i].GetID() > _currentID ||
                         featureVectors[i].GetID() < _currentID - queryRateInFrames) &&
                        featureVectors[i].GetFrame() + queryRateInFrames <= featureVectors[i].GetFrameCountForID() &&
                        !banQueue.Contains(featureVectors[i].GetID()))
                    {
                        float comparison = featureVectors[i].GetTrajectory().CompareTrajectories(movementTraj,
                            transform.worldToLocalMatrix.inverse, weightTrajPositions, weightTrajForwards);
                        for (int j = 0; j < candidatesPerMisc; j++)
                        {
                            if (possibleCandidates[j] != null)
                            {
                                if (comparison < candidateValues[j])
                                {
                                    possibleCandidates.Insert(j, featureVectors[i]);
                                    possibleCandidates.RemoveAt(candidatesPerMisc);
                                    candidateValues.Insert(j, comparison);
                                    candidateValues.RemoveAt(candidatesPerMisc);
                                    break;
                                }
                            }
                            else
                            {
                                possibleCandidates[j] = featureVectors[i];
                                candidateValues[j] = featureVectors[i].GetTrajectory().CompareTrajectories(movementTraj,
                                    transform.worldToLocalMatrix.inverse, weightTrajPositions, weightTrajForwards);
                                break;
                            }
                        }
                    }
                }
                for (int i = 0; i < possibleCandidates.Count; i++)
                {
                    if (possibleCandidates[i] != null)
                        candidates.Add(possibleCandidates[i]);
                }
            }
            return candidates;
        }

        private int PoseMatching(List<FeatureVector> candidates)
        {
            int bestId = -1;
            float currentDif = float.MaxValue;
            foreach (var candidate in candidates)
            {
                float velDif = featureVectors[_currentID].GetPose().ComparePoses(candidate.GetPose(),
                    transform.worldToLocalMatrix.inverse, weightRootVel, weightLFootVel, weightRFootVel, weightNeckVel);
                float feetPosDif = featureVectors[_currentID].GetPose().GetJointDistance(candidate.GetPose(),
                    transform.worldToLocalMatrix.inverse, weightFeetPos, weightNeckPos);
                float candidateDif = velDif + feetPosDif; // TODO: Look at joint distance for idle
                if (candidateDif < currentDif)
                {
                    //Debug.Log("Candidate ID " + candidate.GetID() + " diff: " + candidateDif + " < " + " Current ID " + bestId + " diff:" + currentDif + "\nVelocity dif was " + velDif + " and feetPos dif was " + feetPosDif);
                    bestId = candidate.GetID();
                    currentDif = candidateDif;
                }
            }

            // TODO: Maybe remove?
            if (featureVectors[bestId].GetClipName() == featureVectors[_currentID].GetClipName() &&
                featureVectors[_currentID].GetFrame() + queryRateInFrames <=
                featureVectors[_currentID].GetFrameCountForID())
                bestId = _currentID;
            return bestId;
        }

        public bool AnimDiscarder(string candidateName, int stateNumber)
        {
            string candidateNameLowerCase = candidateName.ToLower();
            if (candidateNameLowerCase.Contains(states[stateNumber]))
            {
                if (!candidateNameLowerCase.Contains("from" + states[stateNumber]))
                    return true;
            }

            return false;
        }

        public List<FeatureVector> GetFeatureVectors()
        {
            return featureVectors;
        }

        private void OnDisable()
        {
            _trajectoryPositions.Dispose();
            _trajectoryForwards.Dispose();
            _featureIDs.Dispose();
            _featureFrame.Dispose();
            _frameCountForIDs.Dispose();
            _featureState.Dispose();
        }
    }

    [BurstCompile]
    public struct TrajectoryMatchingParallelJob : IJob
    {
        public NativeArray<float3> animPositionArray;
        public NativeArray<float3> animForwardArray;
        public NativeArray<float3> movementPositionArray;
        public NativeArray<float3> movementForwardArray;
        public NativeArray<int> featureIDs;
        public NativeArray<int> featureState;
        public NativeArray<int> featureFrame;
        public NativeArray<int> frameCountForIDs;
        public NativeArray<int> candidateIDs;
        public NativeArray<float> candidateValues;
        public Matrix4x4 movementMatrix;        // TODO: Convert matrices to float4x4
        public Matrix4x4 animationMatrix;
        public int currentID;
        public int state;
        public int queryRate;
        public int trajPoints;

        public float positionWeight,
            forwardWeight;
        public void Execute()
        {
            for (int i = 0; i < featureIDs.Length; i++)
            {
                if (i == 0)
                {
                    for (int j = 0; j < candidateIDs.Length; j++)
                    {
                        candidateValues[j] = float.MaxValue;
                    }
                }
                // TODO: Move animation matrix conversion to Script initialization
                if (featureState[i] == state) // Animation has the desired state
                {
                    if ((featureIDs[i] > currentID || featureIDs[i] < currentID - queryRate) && featureFrame[i] + queryRate <= frameCountForIDs[i] /*&& !banQueue.Contains(featureIDs[i])*/)
                    {
                        float comparison = 0;
                        animationMatrix.SetTRS(animPositionArray[i], Quaternion.identity, Vector3.one);
                        for (int j = 0; j < movementPositionArray.Length; j++)
                        {
                            comparison += math.distancesq(movementMatrix.MultiplyPoint3x4(animationMatrix.MultiplyPoint3x4(animPositionArray[i * trajPoints + j])), movementPositionArray[j]) * positionWeight;
                            comparison += math.distancesq(movementMatrix.MultiplyPoint3x4(animationMatrix.MultiplyPoint3x4(animPositionArray[i * trajPoints + j])), movementPositionArray[j]) * forwardWeight;
                        }

                        // Array shifting if comparison is less than any value in the array
                        for (int j = candidateIDs.Length - 1; j >= 0; j--)
                        {
                            if (comparison >= candidateValues[j] || j == 0)
                            {
                                if (j == candidateIDs.Length - 1)
                                    break;

                                for (int k = candidateIDs.Length - 1; k > j; k--)
                                {
                                    candidateIDs[k] = candidateIDs[k - 1];
                                    candidateValues[k] = candidateValues[k - 1];
                                }
                                candidateIDs[j] = featureIDs[i];
                                candidateValues[j] = comparison;
                            }
                        }
                    }
                }
            }
        }
    }
}