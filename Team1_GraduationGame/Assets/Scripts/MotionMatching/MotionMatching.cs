// Code Owner: Jannik Neerdal
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
        // TODO: Fix release of input resulting in no smoothed stop trajectory (instant)
 
        // Should have
        // TODO: Extrapolate empty trajectorypoints (points that go over the frame size for that clip)

        // Nice to have
        // TODO: Add idle events
        // TODO: Add method summaries and general documentation 

        // Out of scope
        // TODO: Collision detection with raycasting between the trajectory points
        // TODO: Create LookUp system in preproccesing, that can be used instead of pose matching during runtime
        // TODO: Create some debugger that shows various information about the data, especially the trajectory for each frame

        // INSPECTOR
        [Header("Settings")]
        public int pointsPerTrajectory = 4;
        public int framesBetweenTrajectoryPoints = 10;
        [SerializeField] private int queryRateInFrames = 10, candidatesFromTrajectory = 10, banQueueSize = 10;

        [Tooltip("The time in frames that it takes for the animation layers to blend.")]
        [SerializeField] private float animLayerWeightChange = 5.0f;
        [SerializeField] private float animationFrameRate = 50.0f;

        [Header("Weights")]
        [Range(0, 1)] [SerializeField] private float weightRootVel = 1.0f;
        [Range(0, 1)] [SerializeField] private float weightFeetVel = 1.0f,
            weightNeckVel = 1.0f,
            weightNeckPos = 1.0f,
            weightFeetPos = 1.0f,
            weightTrajPositions = 1.0f,
            weightTrajForwards = 1.0f;

        [Header("Preprocessing")]
        public HumanBodyBones[] joints =
        {
            HumanBodyBones.Hips,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightFoot,
            HumanBodyBones.Neck
        };
        public AnimContainer animContainer; // put ref to chosen animation container scriptable object
        [SerializeField] private string[] states =
        {
            "Idle",
            "Sneak",
            "Walk",
            "Run"
        };
        [SerializeField] private bool _preProcess, _loadAnimationsFromController;

        // HIDDEN
        // --- References
        private Movement movement;
        private PreProcessing preProcessing;
        private Animator animator;

        // --- Collections
        private AnimationClip[] allClips;
        private List<FeatureVector> featureVectors, 
            _trajCandidatesRef = new List<FeatureVector>(), 
            _trajPossibleCandidatesRef = new List<FeatureVector>();
        private List<float> _trajCandidateValuesRef = new List<float>();
        private NativeArray<float3> _trajectoryPositions, 
            _trajectoryForwards;
        private NativeArray<int> _featureIDs,
            _featureFrame, 
            _frameCountForIDs, 
            _featureState;
        private NativeList<int> _bannedIDs;

        // --- Variables
        private AnimationClip _currentClip;
        private int _currentID,
            _queueCounter,
            _fixedUpdateMMCounter,
            _changeLayerIndex,
            _changeLayerDesiredWeight;
        private bool changeAnimLayer;
        private float _fixedUpdateChangeAnimLayerCounter,
            _changeLayerCurrentWeight;

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
                if (_loadAnimationsFromController)
                {
                    // Get animations from animation controller, and store it in a scriptable object
                    if (animator != null)
                    {
                        allClips = animator.runtimeAnimatorController.animationClips;
                        if (allClips == null)
                            Debug.LogError("Durnig preprocessing, tried to find animation clips in the animator controller, but there was none!");
                    }
                    else
                    {
                        Debug.LogError("No Animator was found in the supplied GameObject during mm preprocessing!", gameObject);
                    }

                    AnimContainer tempAnimContainer = new AnimContainer();
                    tempAnimContainer.animationClips = allClips;
                    EditorUtility.CopySerialized(tempAnimContainer, animContainer);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    allClips = animContainer.animationClips;
                }
                preProcessing.Preprocess(allClips, joints, gameObject, animator, animationFrameRate, states);
            }
#endif
            allClips = animContainer.animationClips;
            featureVectors = preProcessing.LoadData(pointsPerTrajectory, framesBetweenTrajectoryPoints);
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
            _bannedIDs = new NativeList<int>(banQueueSize, Allocator.Persistent);

            // Persistent native array data allocation
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

            _fixedUpdateMMCounter = queryRateInFrames;
        }

        private void FixedUpdate()
        {
            if (_fixedUpdateMMCounter >= queryRateInFrames)
            {
                _currentID += queryRateInFrames;
                List<FeatureVector> candidates = TrajectoryMatching(movement.GetMovementTrajectory(), ref _trajCandidatesRef, ref _trajPossibleCandidatesRef, ref _trajCandidateValuesRef);
                int candidateID = PoseMatching(candidates);
                UpdateAnimation(candidateID, featureVectors[candidateID].GetFrame());
                _fixedUpdateMMCounter = 0;
            }
            _fixedUpdateMMCounter++;

            if (changeAnimLayer)
            {
                _changeLayerCurrentWeight = math.lerp(_changeLayerCurrentWeight, _changeLayerDesiredWeight, _fixedUpdateChangeAnimLayerCounter / animLayerWeightChange);
                animator.SetLayerWeight(_changeLayerIndex, _changeLayerCurrentWeight);
                _fixedUpdateChangeAnimLayerCounter++;
                if (_changeLayerCurrentWeight == _changeLayerDesiredWeight)
                {
                    _fixedUpdateChangeAnimLayerCounter = 0;
                    changeAnimLayer = false;
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Matrix4x4 invCharSpace = transform.worldToLocalMatrix.inverse;
                Gizmos.color = Color.red; // Movement Trajectory
                for (int i = 0; i < movement.GetMovementTrajectory().GetTrajectoryPoints().Length; i++) // Gizmos for movement
                {
                    // Position
                    Gizmos.DrawWireSphere(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(), 0.1f);
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
                    Gizmos.DrawWireSphere(invCharSpace.MultiplyPoint3x4(featureVectors[_currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint()), 0.1f);

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
#endif

        public void ChangeLayerWeight(int layerIndex, int desiredWeight)
        {
            changeAnimLayer = true;
            _changeLayerIndex = layerIndex;
            _changeLayerDesiredWeight = desiredWeight;
            _changeLayerCurrentWeight = animator.GetLayerWeight(_changeLayerIndex);
        }

        private void UpdateAnimation(int id, int frame)
        {
            for (int i = 0; i < allClips.Length; i++)
            {
                if (allClips[i].name == featureVectors[id].GetClipName())
                {
                    _currentClip = allClips[i];
                    break;
                }
            }

            //Debug.Log("Updating animation: ID: " + _currentID + " -> " + id + " | Frame: " + (featureVectors[_currentID].GetFrame() + queryRateInFrames) + " -> " + frame + " | Name: " + featureVectors[_currentID].GetClipName() + " -> " + _currentClip.name + ".");
            animator.CrossFadeInFixedTime(_currentClip.name, queryRateInFrames / animationFrameRate, 0, frame / animationFrameRate); // 0.3f was recommended by Magnus
            _currentID = id;
            _bannedIDs.Add(id);
            if (_bannedIDs.Length > banQueueSize)
            {
                _bannedIDs.RemoveAtSwapBack(_queueCounter);
                _queueCounter++;
                if (_queueCounter >= banQueueSize)
                    _queueCounter = 0;
            }
        }

        List<FeatureVector> TrajectoryMatching(Trajectory movementTraj, ref List<FeatureVector> candidates, ref List<FeatureVector> possibleCandidates, ref List<float> candidateValues)
        {
            candidates.Clear();
            NativeArray<int> trajectoryCandidateIDs = new NativeArray<int>(candidatesFromTrajectory, Allocator.TempJob);
            NativeArray<float> trajectoryCandidateValues = new NativeArray<float>(candidatesFromTrajectory, Allocator.TempJob);
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
                bannedIDs = _bannedIDs,
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
            return candidates;
        }

        private int PoseMatching(List<FeatureVector> candidates)
        {
            Matrix4x4 charSpace = transform.worldToLocalMatrix.inverse;
            MMPose currentPose = featureVectors[_currentID].GetPose();
            int poseFeatureCount = 7;
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
                cPose_rightFootVel = charSpace.MultiplyPoint3x4(currentPose.GetRightFootVelocity()),
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
            int bestID = bestIDArray[0];
            bestIDArray.Dispose();
            candidateIDs.Dispose();
            candidatesPoseData.Dispose();

            //// This part basically autoplays animations if the candidate is from the same clip, and it is not at the end of the animation
            //if (featureVectors[bestID].GetClipName() == featureVectors[_currentID].GetClipName() &&
            //    featureVectors[_currentID].GetFrame() + queryRateInFrames <=
            //    featureVectors[_currentID].GetFrameCountForID())
            //    bestID = _currentID;
            return bestID;
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
            _bannedIDs.Dispose();
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
        public NativeList<int> bannedIDs;
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
            for (int i = 0; i < featureIDs.Length; i++)
            {
                if (featureState[i] == state) // Animation has the desired state
                {
                    if ((featureIDs[i] > currentID || featureIDs[i] < currentID - queryRate * 2) && featureFrame[i] + queryRate <= frameCountForIDs[i] && !bannedIDs.Contains(featureIDs[i]))
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