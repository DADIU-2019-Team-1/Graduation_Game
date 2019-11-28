// Code Owner: Jannik Neerdal

using System;
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

namespace Team1_GraduationGame.MotionMatching
{
    [RequireComponent(typeof(Animator))]
    public class MotionMatching : MonoBehaviour
    {
        // TODOs
        // Must have
        // TODO: Revise pose matching
        // TODO: Cut down idle animations (some repetition)

        // Should have
        // TODO: Extrapolate empty trajectorypoints (points that go over the frame size for that clip)
        // TODO: Collision detection with raycasting between the trajectory points

        // Nice to have
        // TODO: Create LookUp system in preproccesing, that can be used instead of pose matching during runtime
        // TODO: Create some debugger that shows various information about the data, especially the trajectory for each frame
        // TODO: When preprocessing, also store the data that is being written to CSV as return to feature vector (do load and write step together when preprocessing)
        // TODO: Add idle events
        // TODO: Add method summaries and general documentation 


        // --- References
        private Movement movement;
        private PreProcessing preProcessing;
        private Animator animator;

        // --- Collections
        private List<FeatureVector>[] featureVectorStates;
        private List<FeatureVector> featureVectors, _trajCandidatesRef, _trajPossibleCandidatesRef;
        private List<float> _trajCandidateValuesRef;
        private AnimationClip[] allClips;
        [Header("Collections")]
        public HumanBodyBones[] joints;
        public AnimContainer animContainer; // put ref to chosen animation container scriptable object
        [SerializeField] private string[] states;

        // --- Variables
        [Header("Settings")]
        [SerializeField] private bool useJobs = true;
        public bool _preProcess, _playAnimationMode;
        public int pointsPerTrajectory = 4, framesBetweenTrajectoryPoints = 10;
        [SerializeField] private int queryRateInFrames = 10, candidatesPerMisc = 10, banQueueSize = 10;

        [Tooltip("The time in frames that it takes for the animation layers to blend.")] 
        [SerializeField] private float animLayerWeightChange = 5.0f;

        private AnimationClip currentClip;
        private int currentFrame, _currentID;
        [SerializeField] private float animationFrameRate = 50.0f;

        [Header("Weights")] 
        [Range(0, 1)] public float weightRootVel = 1.0f;
        [Range(0, 1)] public float weightFeetVel = 1.0f,
            weightNeckVel = 1.0f,
            weightNeckPos = 1.0f,
            weightFeetPos = 1.0f,
            weightTrajPositions = 1.0f,
            weightTrajForwards = 1.0f;

        // --- Debugstuff
        [Header("Debug")] 
        private int animIterator = -1;
        private Coroutine currentCoroutine = null;
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
                int frames = (int)(allClips[i].length * animationFrameRate);
                for (int j = 0; j < featureVectors.Count; j++)
                {
                    if (featureVectors[j].GetClipName() == allClips[i].name)
                    {
                        featureVectors[j].SetFrameCount(frames);
                    }
                }
            }

            featureVectorStates = new List<FeatureVector>[states.Length];
            for (int j = 0; j < states.Length; j++)
            {
                featureVectorStates[j] = new List<FeatureVector>();
            }
            for (int i = 0; i < featureVectors.Count; i++)
            {
                int featureState = featureVectors[i].GetState();
                featureVectorStates[featureState].Add(featureVectors[i]);
            }

            _trajCandidatesRef = new List<FeatureVector>();
            _trajPossibleCandidatesRef = new List<FeatureVector>();
            _trajCandidateValuesRef = new List<float>();
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
                    _trajectoryPositions[i * pointsPerTrajectory + j] = featureVectors[i].GetTrajectory().GetTrajectoryPoints()[j].GetPoint();
                    _trajectoryForwards[i * pointsPerTrajectory + j] = featureVectors[i].GetTrajectory().GetTrajectoryPoints()[j].GetForward();
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
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(ChangeLayerWeightOverTime(layerIndex, desiredWeight));
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
                if (Input.GetKeyDown(KeyCode.B))
                {
                    if (currentCoroutine != null)
                        StopCoroutine(currentCoroutine);
                    transform.position = Vector3.zero;
                    currentCoroutine = StartCoroutine(PlayAnimation());
                }
            }
        }
#endif

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Matrix4x4 invCharSpace = transform.worldToLocalMatrix.inverse;
                Gizmos.color = Color.red; // Movement Trajectory
                for (int i = 0; i < movement.GetMovementTrajectory().GetTrajectoryPoints().Length; i++) // Gizmos for movement
                {
                    // Position
                    Gizmos.DrawWireSphere(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(), 0.2f);
                    if (i != 0)
                    {
                        Gizmos.DrawLine(movement.GetMovementTrajectory().GetTrajectoryPoints()[i - 1].GetPoint(), 
                            movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint());
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
                    Gizmos.DrawWireSphere(invCharSpace.MultiplyPoint3x4(featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint()), 0.2f);

                    if (i != 0)
                    {
                        Gizmos.DrawLine(invCharSpace.MultiplyPoint3x4(featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i - 1].GetPoint()),
                            invCharSpace.MultiplyPoint3x4(featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint()));
                    }

                    // Forward
                    Gizmos.DrawLine(
                        invCharSpace.MultiplyPoint3x4(featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint()),
                        invCharSpace.MultiplyPoint3x4(featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint()) +
                        invCharSpace.MultiplyVector(featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetForward()));
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

            Debug.Log("Updating animation: ID: " + _currentID + " -> " + id + " | Frame: " + (currentFrame + queryRateInFrames) + " -> " + frame + " | Name: " + currentClip.name + " -> " + featureVectors[_currentID].GetClipName() + ".");
            animator.CrossFadeInFixedTime(currentClip.name, queryRateInFrames / animationFrameRate/* * 0.3f*/, 0, frame / animationFrameRate); // 0.3f was recommended by Magnus
            _currentID = id;
            currentFrame = frame;
        }

        private IEnumerator MotionMatch()
        {
            while (true)
            {
                _currentID += queryRateInFrames;
                List<FeatureVector> candidates = TrajectoryMatching(movement.GetMovementTrajectory(),ref _trajCandidatesRef, ref _trajPossibleCandidatesRef, ref _trajCandidateValuesRef);
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
                Debug.Log("Finished playing last animation! Going back to first animation...");
            }
            Debug.Log("Playing animation " + allClips[animIterator]);

            for (int i = 0; i < featureVectors.Count; i++)
            {
                if (featureVectors[i].GetClipName() == allClips[animIterator].name)
                {
                    _currentID = i;
                    break;
                }
            }

            int startofIdForClip = _currentID;
            int animIteratorTwo = animIterator;
            Debug.Log("Current playing " + allClips[animIterator].name + ", which is " + allClips[animIterator].length +
                      " seconds long! Starting at ID " + startofIdForClip);
            Debug.Log("While loop condition: Clip " + featureVectors[_currentID].GetClipName() + " == " + allClips[animIterator].name);
            while (featureVectors[_currentID].GetClipName() == allClips[animIterator].name)
            {
                Debug.Log("Current ID is now " + _currentID + ", which started at ID " + startofIdForClip + "!");
                UpdateAnimation(_currentID, featureVectors[_currentID].GetFrame());
                yield return new WaitForSeconds(queryRateInFrames / animationFrameRate);
                _currentID += queryRateInFrames;
            }
        }

        List<FeatureVector> TrajectoryMatching(Trajectory movementTraj, ref List<FeatureVector> candidates, ref List<FeatureVector> possibleCandidates, ref List<float> candidateValues)
        {
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

                TrajectoryMatchingJob trajectoryJob = new TrajectoryMatchingJob
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
                    movementMatrix = transform.worldToLocalMatrix.inverse,
                    positionWeight = weightTrajPositions,
                    forwardWeight = weightTrajForwards
                };
                JobHandle handle = trajectoryJob.Schedule();
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
                        featureVectors[i].GetFrame() + queryRateInFrames <= featureVectors[i].GetFrameCountForID())
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
            int bestID = -1;
            float currentDif = float.MaxValue;
            string printToConsoleLog = "";
            Matrix4x4 charSpace = transform.worldToLocalMatrix.inverse;
            MMPose currentPose = featureVectors[_currentID].GetPose();
            int poseFeatureCount = 7;

            if (useJobs)
            {
                NativeArray<int> bestIDArray = new NativeArray<int>(1, Allocator.TempJob);
                NativeArray<int> candidateIDs = new NativeArray<int>(candidates.Count, Allocator.TempJob);
                NativeArray<float3> candidatesPoseData = new NativeArray<float3>(candidates.Count * poseFeatureCount, Allocator.TempJob);

                for (int i = 0; i < candidatesPoseData.Length; i += poseFeatureCount)
                {
                    MMPose candidatePose = candidates[i / poseFeatureCount].GetPose();
                    candidateIDs[i / poseFeatureCount] = candidates[i / poseFeatureCount].GetID();
                    candidatesPoseData[i] = candidatePose.GetRootVelocity();
                    candidatesPoseData[i + 1] = candidatePose.GetLeftFootVelocity();
                    candidatesPoseData[i + 2] = candidatePose.GetRightFootVelocity();
                    candidatesPoseData[i + 3] = candidatePose.GetNeckVelocity();
                    candidatesPoseData[i + 4] = candidatePose.GetLeftFootPos();
                    candidatesPoseData[i + 5] = candidatePose.GetRightFootPos();
                    candidatesPoseData[i + 6] = candidatePose.GetNeckPos();
                }

                PoseMatchingJob poseJob = new PoseMatchingJob
                {
                    charSpace = charSpace,
                    chunkLength = poseFeatureCount,
                    bestID = bestIDArray,
                    candidateIDs = candidateIDs,
                    cPose_rootVel = charSpace.MultiplyPoint3x4(currentPose.GetRootVelocity()),
                    cPose_leftFootVel = charSpace.MultiplyPoint3x4(currentPose.GetLeftFootVelocity()),
                    cPose_rightFootVel = charSpace.MultiplyPoint3x4(currentPose.GetRightFootPos()),
                    cPose_neckVel = charSpace.MultiplyPoint3x4(currentPose.GetNeckVelocity()),
                    cPose_leftFootPos = charSpace.MultiplyPoint3x4(currentPose.GetLeftFootPos()),
                    cPose_rightFootPos = charSpace.MultiplyPoint3x4(currentPose.GetRightFootPos()),
                    cPose_neckPos = charSpace.MultiplyPoint3x4(currentPose.GetNeckPos()),
                    candidatesPoseData = candidatesPoseData,
                    rootVelWeight = weightRootVel,
                    feetVelWeight = weightFeetVel,
                    neckVelWeight = weightNeckVel,
                    feetPosWeight = weightFeetPos,
                    neckPosWeight = weightNeckPos
                };
                JobHandle handle = poseJob.Schedule();
                handle.Complete();
                bestID = bestIDArray[0];
                bestIDArray.Dispose();
                candidateIDs.Dispose();
                candidatesPoseData.Dispose();
            }
            else
            {
                foreach (var candidate in candidates)
                {
                    MMPose candidatePose = candidate.GetPose();
                    float velocityDiff = math.pow(Vector3.Distance(charSpace.MultiplyPoint3x4(currentPose.GetRootVelocity()),
                                        charSpace.MultiplyPoint3x4(candidatePose.GetRootVelocity())) * weightRootVel + 1, 2);
                    velocityDiff += math.pow(Vector3.Distance(charSpace.MultiplyPoint3x4(currentPose.GetLeftFootVelocity()),
                                        charSpace.MultiplyPoint3x4(candidatePose.GetLeftFootVelocity())) * weightFeetVel + 1, 2);
                    velocityDiff += math.pow(Vector3.Distance(charSpace.MultiplyPoint3x4(currentPose.GetRightFootVelocity()),
                                        charSpace.MultiplyPoint3x4(candidatePose.GetRightFootVelocity())) * weightFeetVel + 1, 2);
                    velocityDiff += math.pow(Vector3.Distance(charSpace.MultiplyPoint3x4(currentPose.GetNeckVelocity()),
                                        charSpace.MultiplyPoint3x4(candidatePose.GetNeckVelocity())) * weightNeckVel + 1, 2);

                    float positionDif = math.pow(Vector3.Distance(charSpace.MultiplyPoint3x4(currentPose.GetLeftFootPos()), charSpace.MultiplyPoint3x4(candidatePose.GetLeftFootPos())) * weightFeetPos + 1, 2);
                    positionDif += math.pow(Vector3.Distance(charSpace.MultiplyPoint3x4(currentPose.GetRightFootPos()), charSpace.MultiplyPoint3x4(candidatePose.GetRightFootPos())) * weightFeetPos + 1, 2);
                    positionDif += math.pow(Vector3.Distance(charSpace.MultiplyPoint3x4(currentPose.GetNeckPos()), charSpace.MultiplyPoint3x4(candidatePose.GetNeckPos())) * weightNeckPos + 1, 2);

                    float candidateDif = velocityDiff + positionDif;
                    printToConsoleLog += "ID: " + candidate.GetID() + " diff: " + candidateDif + " | ";
                    if (candidateDif < currentDif)
                    {
                        Debug.Log("Candidate ID " + candidate.GetID() + " diff: " + (candidateDif - 7) + " < " + " Current ID " + bestID + " diff:" + (currentDif - 7) + "\nVelocity dif was " + (velocityDiff - 4) + " and feetPos dif was " + (positionDif - 3));
                        bestID = candidate.GetID();
                        currentDif = candidateDif;
                    }
                }
            }
            // TODO: Maybe remove?
            //if (featureVectors[bestID].GetClipName() == featureVectors[_currentID].GetClipName() &&
            //    featureVectors[_currentID].GetFrame() + queryRateInFrames * 5.0f <=
            //    featureVectors[_currentID].GetFrameCountForID())
            //    bestID = _currentID;
            return bestID;
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
    public struct TrajectoryMatchingJob : IJob
    {
        public NativeArray<float3> animPositionArray,
            animForwardArray,
            movementPositionArray,
            movementForwardArray;
        public NativeArray<int> featureIDs,
            featureState,
            featureFrame,
            frameCountForIDs,
            candidateIDs;
        public NativeArray<float> candidateValues;
        public Matrix4x4 movementMatrix,
            animationMatrix;
        public int currentID,
            state,
            queryRate,
            trajPoints;

        public float positionWeight,
            forwardWeight;
        public void Execute()
        {
            for (int j = 0; j < candidateIDs.Length; j++)
            {
                candidateValues[j] = float.MaxValue;
            }

            int enam = 0;
            int enamsingle = 0;

            for (int i = 0; i < featureIDs.Length; i++)
            {
                if (featureState[i] == state) // Animation has the desired state
                {
                    if ((featureIDs[i] > currentID || featureIDs[i] < currentID - queryRate) && featureFrame[i] + queryRate <= frameCountForIDs[i] /*&& !banQueue.Contains(featureIDs[i])*/)
                    {
                        float comparison = 0;
                        animationMatrix.SetTRS(animPositionArray[i], Quaternion.identity, Vector3.one);
                        for (int j = 0; j < movementPositionArray.Length; j++)
                        {
                            comparison += math.pow(math.distancesq(movementMatrix.MultiplyPoint3x4(animPositionArray[i * trajPoints + j]), movementPositionArray[j]) * positionWeight + 1,2);
                            comparison += math.pow(math.distancesq(movementMatrix.MultiplyVector(animForwardArray[i * trajPoints + j]), movementForwardArray[j]) * forwardWeight + 1,2);
                        }

                        //Array shifting if comparison is less than any value in the array
                        if (comparison < candidateValues[candidateValues.Length - 1])
                        {
                            int j = candidateValues.Length - 1;
                            for (; j >= 0 && comparison < candidateValues[j]; j--) { }
                            j++;
                            for (int k = candidateValues.Length - 1; k > j; k--)
                            {
                                candidateValues[k] = candidateValues[k - 1];
                                candidateIDs[k] = candidateIDs[k - 1];
                            }
                            candidateValues[j] = comparison;
                            candidateIDs[j] = featureIDs[i];
                        }
                    }
                }
            }
        }
    }

    [BurstCompile]
    public struct PoseMatchingJob : IJob
    {
        public Matrix4x4 charSpace;
        public int chunkLength;
        public float3 cPose_rootVel,
            cPose_leftFootVel,
            cPose_rightFootVel,
            cPose_neckVel,
            cPose_leftFootPos,
            cPose_rightFootPos,
            cPose_neckPos;
        public NativeArray<int> bestID, candidateIDs;
        public NativeArray<float3> candidatesPoseData;
        public float rootVelWeight,
            feetVelWeight,
            neckVelWeight,
            feetPosWeight,
            neckPosWeight;
        public void Execute()
        {
            float currentDiff = float.MaxValue;
            for (int i = 0; i < candidatesPoseData.Length; i += chunkLength)
            {
                float velocityDiffs = math.pow(math.distancesq(cPose_rootVel, charSpace.MultiplyPoint3x4(candidatesPoseData[i])) * rootVelWeight + 1, 2);
                velocityDiffs += math.pow(math.distancesq(cPose_leftFootVel, charSpace.MultiplyPoint3x4(candidatesPoseData[i + 1])) * feetVelWeight + 1, 2);
                velocityDiffs += math.pow(math.distancesq(cPose_rightFootVel, charSpace.MultiplyPoint3x4(candidatesPoseData[i + 2])) * feetVelWeight + 1, 2);
                velocityDiffs += math.pow(math.distancesq(cPose_neckVel, charSpace.MultiplyPoint3x4(candidatesPoseData[i + 3])) * neckVelWeight + 1, 2);

                float positionDiffs = math.pow(math.distancesq(cPose_leftFootPos, charSpace.MultiplyPoint3x4(candidatesPoseData[i + 4])) * feetPosWeight + 1, 2);
                positionDiffs += math.pow(math.distancesq(cPose_rightFootPos, charSpace.MultiplyPoint3x4(candidatesPoseData[i + 5])) * feetPosWeight + 1, 2);
                positionDiffs += math.pow(math.distancesq(cPose_neckPos, charSpace.MultiplyPoint3x4(candidatesPoseData[i + 6])) * neckPosWeight + 1, 2);

                float candidateDiff = velocityDiffs + positionDiffs;
                if (candidateDiff < currentDiff)
                {
                    bestID[0] = candidateIDs[i / chunkLength];
                    currentDiff = candidateDiff;
                }
            }
        }
    }
}