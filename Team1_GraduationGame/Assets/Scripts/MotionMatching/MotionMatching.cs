using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MotionMatching : MonoBehaviour
{
    // TODO: Add method summaries and general documentation 
    // TODO: Create LookUp system in preproccesing, that can be used instead of pose matching during runtime
    // TODO: Convert system to Unity DOTS - can only take NativeArrays<float3>
    // TODO: When preprocessing, also store the data that is being written to CSV as return to feature vector (do load and write step together when preprocessing)
    // TODO: Do correct char space conversion
    // TODO: Check that forwards for trajectories are being created correctly
    // TODO: Create bool for using misc or not, since our current misc system doesn't really make sense
    // TODO: Create some debugger that shows various information about the data, especially the trajectory for each frame
    // TODO: Collision detection with raycasting between the trajectory points
    // TODO: Extrapolate empty trajectorypoints (points that go over the frame size for that clip)

    // TODO: https://docs.unity3d.com/ScriptReference/AnimationClip.SampleAnimation.html
    // TODO: https://docs.unity3d.com/ScriptReference/HumanBodyBones.html

	// BUG: CSV Data is not correctly converted to char space (feet are in world pos)


    // --- References
    private MovementTest movement;
    private PreProcessing preProcessing;
    private Animator animator;

    // --- Collections
    private List<FeatureVector> featureVectors, trajCandidatesRef, trajPossibleCandidatesRef;
    private AnimationClip[] allClips;
    public HumanBodyBones[] joints;
    public AnimContainer animContainer; // put ref to chosen animation container scriptable object
    public string[][] movementTags = // TODO: Create an inspector version of this
    {
        new []{ "Idle"},                        // State 0
        new []{ "Sneak", "Walk", "Run" }        // State 1
    };

    [SerializeField] private string[] states;

    private List<bool> enumeratorBools;

    // --- Variables 
    public bool _preProcess, _playAnimationMode;
    public int pointsPerTrajectory = 4, framesBetweenTrajectoryPoints = 10;
    public float idleThreshold = 0.10f;
    [SerializeField] private bool _isMotionMatching, _isIdling;
    [SerializeField] private int queryRateInFrames = 10, candidatesPerMisc = 10;

    private AnimationClip currentClip;
    private int currentFrame, currentID, currentState;

    // --- Weights
    [Range(0, 1)] public float weightRootVel = 1.0f, weightLFootVel = 1.0f, weightRFootVel = 1.0f, weightNeckVel = 1.0f,
	    weightNeckPos = 1.0f, weightFeetPos = 1.0f, weightTrajPoints = 1.0f, weightTrajForwards = 1.0f;

    // --- Debugstuff
    private int animIterator = -1;
    private IEnumerator currentEnumerator;

    private void Awake() // Load animation data
    {
        movement = GetComponent<MovementTest>();
	    animator = GetComponent<Animator>();
        preProcessing = new PreProcessing();

        allClips = animContainer.animationClips;
#if UNITY_EDITOR
        if (_preProcess)
        {
            // Get animations from animation controller, and store it in a scriptable object
            if (animator != null)
            {
                allClips = animator.runtimeAnimatorController.animationClips;
            }
            else
            {
                Debug.LogError("No Animator was found in the supplied GameObject during mm preprocessing!", gameObject);
            }
            AnimContainer tempAnimContainer = new AnimContainer();
            tempAnimContainer.animationClips = allClips;
            EditorUtility.CopySerialized(tempAnimContainer, animContainer);
            AssetDatabase.SaveAssets();

            preProcessing.Preprocess(allClips, joints, gameObject, animator);
        }

        if (allClips == null)
            Debug.LogError("AnimationClips load error: selected scriptable object file empty or none referenced");
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
        
        trajCandidatesRef = new List<FeatureVector>();
        trajPossibleCandidatesRef = new List<FeatureVector>();

        for (int i = 0; i < movementTags.Length; i++)
        {
            for (int j = 0; j < movementTags[i].Length; j++)
            {
                movementTags[i][j] = movementTags[i][j].ToLower();
            }
        }
        enumeratorBools = AddEnumeratorBoolsToList();
    }

    private void Start()
    {
        if (!_playAnimationMode)
        {
	        UpdateAnimation(0, 0);
	        StartCoroutine(MotionMatch());
        }
    }

    private void FixedUpdate()
    {
        if (!_playAnimationMode)
	    {
		    if (movement.GetSpeed() <= idleThreshold && currentState != 0)
			    currentState = 0;
		    else if (movement.GetSpeed() > idleThreshold && currentState != 1)
			    currentState = 1;
		    if (!_isMotionMatching /* && movement.GetSpeed() > idleThreshold*/)
		    {
			    StopAllCoroutines();
			    StartCoroutine(MotionMatch());
		    }
		    //if (!_isIdling && movement.GetSpeed() <= idleThreshold)
		    //{
		    //    StopAllCoroutines();
		    //    StartCoroutine(Idle());
		    //}
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (_playAnimationMode)
	    {
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
		    Matrix4x4 charSpace = transform.localToWorldMatrix;
		    Matrix4x4 animSpace = new Matrix4x4();
            animSpace.SetTRS(featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[0].GetPoint(), Quaternion.identity, Vector3.one);

            Gizmos.color = Color.red; // Movement Trajectory
            for (int i = 0; i < movement.GetMovementTrajectory().GetTrajectoryPoints().Length; i++) // Gizmos for movement
            {
                // Position
                Gizmos.DrawWireSphere(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(), 0.2f);
                Gizmos.DrawLine(i != 0 ? movement.GetMovementTrajectory().GetTrajectoryPoints()[i - 1].GetPoint() : transform.position,
                    movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint());

                // Forward
                Gizmos.DrawLine(movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(),
                    movement.GetMovementTrajectory().GetTrajectoryPoints()[i].GetForward());
            }

            Gizmos.color = Color.green; // Animation Trajectory
            for (int i = 0; i < featureVectors[currentID].GetTrajectory().GetTrajectoryPoints().Length; i++)
            { // TODO: Figure out why anim traj extends when sprinting...
              // Position
              Gizmos.DrawWireSphere(invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint())), 0.2f);

                if (i != 0)
                {
                    Gizmos.DrawLine(invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[i - 1].GetPoint())),
                        invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint())));
                }

                // Forward
                Gizmos.DrawLine(invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[i].GetPoint())),
                    invCharSpace.MultiplyPoint3x4(animSpace.inverse.MultiplyPoint3x4(featureVectors[currentID].GetTrajectory().GetTrajectoryPoints()[i].GetForward())));
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
		Debug.Log("Updating animation " + currentClip.name + " to frame " + frame + " with ID " + id);
        animator.CrossFadeInFixedTime(currentClip.name, 0.3f, 0, frame / currentClip.frameRate); // 0.3f was recommended by Magnus
        currentID = id;
        currentFrame = frame;
    }

    #region IEnumerators
    private List<bool> AddEnumeratorBoolsToList()
    {
		List<bool> list = new List<bool>();
		list = new List<bool>();
		list.Add(_isMotionMatching);
		list.Add(_isIdling);
		return list;
    }
    private void SetBoolsInList(List<bool> list, bool booleanVal)
    {
	    for (int i = 0; i < list.Count; i++)
		    list[i] = booleanVal;
    }

    private IEnumerator MotionMatch()
    {
        SetBoolsInList(enumeratorBools, false);
	    _isMotionMatching = true;
	    while (true)
	    {
		    currentID += queryRateInFrames;
            List<FeatureVector> candidates = TrajectoryMatching(movement.GetMovementTrajectory(), ref trajCandidatesRef, ref trajPossibleCandidatesRef);
            int candidateID = PoseMatching(candidates);
			UpdateAnimation(candidateID, featureVectors[candidateID].GetFrame());
            yield return new WaitForSeconds(queryRateInFrames / currentClip.frameRate);
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
			    currentID = i;
			    break;
            }
	    }
	    int startofIdForClip = currentID;
	    Debug.Log("Current playing " + allClips[animIterator].name + ", which is " + allClips[animIterator].length + " seconds long!");
		Debug.Log("While loop condition: Frame " + featureVectors[currentID].GetFrame() + " < " + featureVectors[currentID].GetFrameCountForID() + " && Clip " + featureVectors[currentID].GetClipName() + " == " + allClips[animIterator].name);
        while (featureVectors[currentID].GetFrame() < featureVectors[currentID].GetFrameCountForID() && featureVectors[currentID].GetClipName() == allClips[animIterator].name)
        {
            Debug.Log("Current ID is now " + currentID + ", which started at ID " + startofIdForClip +"!");
		    UpdateAnimation(currentID, featureVectors[currentID].GetFrame());
            yield return new WaitForSeconds(queryRateInFrames / currentClip.frameRate);
            currentID += queryRateInFrames;
        }
    }
    #endregion

    List<FeatureVector> TrajectoryMatching(Trajectory movementTraj, ref List<FeatureVector> candidates, ref List<FeatureVector> possibleCandidates)
    {
        candidates.Clear();
        possibleCandidates.Clear();
        List<float> values = new List<float>();
        for (int i = 0; i < candidatesPerMisc; i++)
        {
            possibleCandidates.Add(null);
            values.Add(float.MaxValue);
        }

        for (int i = 0; i < featureVectors.Count; i++)
		{
            if (!TagChecker(featureVectors[i].GetClipName(), currentState))
                continue;
            if ((featureVectors[i].GetID() > currentID ||  featureVectors[i].GetID() < currentID - queryRateInFrames) &&
                 featureVectors[i].GetFrame() + queryRateInFrames <  featureVectors[i].GetFrameCountForID())
            {
                float comparison = featureVectors[i].GetTrajectory().CompareTrajectories(movementTraj, transform.worldToLocalMatrix.inverse, weightTrajPoints, weightTrajForwards);
                for (int j = 0; j < candidatesPerMisc; j++)
                {
                    if (possibleCandidates[j] != null)
                    {
                        if (comparison < values[j])
                        {
                            possibleCandidates.Insert(j, featureVectors[i]);
                            possibleCandidates.RemoveAt(candidatesPerMisc);
                            values.Insert(j, comparison);
                            values.RemoveAt(candidatesPerMisc);
                            break;
                        }
                    }
                    else
                    {
                        possibleCandidates[j] = featureVectors[i];
                        values[j] = featureVectors[i].GetTrajectory().CompareTrajectories(movementTraj,
                            transform.worldToLocalMatrix.inverse, weightTrajPoints, weightTrajForwards);
                        break;
                    }
                }
            }
        }
        foreach (var candidate in possibleCandidates)
        {
            if (candidate != null)
                candidates.Add(candidate);
        }
        return candidates;
    }

    private int PoseMatching(List<FeatureVector> candidates)
    {
        int bestId = -1;
        float currentDif = float.MaxValue;
        //Debug.Log("Pose matching for " + candidates.Count + " candidates!");
        foreach (var candidate in candidates)
        {
            float velDif = featureVectors[currentID].GetPose().ComparePoses(candidate.GetPose(), transform.worldToLocalMatrix, weightRootVel, weightLFootVel, weightRFootVel, weightNeckVel);
            float feetPosDif = featureVectors[currentID].GetPose().GetJointDistance(candidate.GetPose(), transform.worldToLocalMatrix, weightFeetPos, weightNeckPos);
            float candidateDif = velDif + feetPosDif;
            if (candidateDif < currentDif)
            {
				//Debug.Log("Candidate diff: " + velDif + " < " + " Current diff:" + currentDif);
                bestId = candidate.GetID();
                currentDif = candidateDif;
            }
        }
		//Debug.Log("Returning best id from pose matching: " + bestId);
		return bestId;
    }

    private bool TagChecker(string candidateName, int stateNumber)
    {
        for (int i = 0; i < movementTags[stateNumber].Length; i++)
        {
            if (candidateName.ToLower().Contains(movementTags[stateNumber][i]))
                return true;
        }
        return false;
    }
    private bool TagChecker(string candidateName, int stateNumber, int miscNumber)
    {
	    if (candidateName.ToLower().Contains(movementTags[stateNumber][miscNumber]))
		    return true;
        return false;
    }
    public List<FeatureVector> GetFeatureVectors()
    {
	    return featureVectors;
    }
}