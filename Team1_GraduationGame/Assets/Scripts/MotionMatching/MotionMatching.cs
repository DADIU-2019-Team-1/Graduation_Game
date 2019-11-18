using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Team1_GraduationGame.Events;
using Team1_GraduationGame.Managers;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MotionMatching : MonoBehaviour
{
    // Must have
    // TODO BUGFIX: Animation forwards are being calculated incorrectly (oppositely?)
    // TODO BUGFIX: Animations do not blend together. Reason could be pose matching, but might be something else
    // TODO: Revise pose matching
    // TODO: Cut down idle animations (lots of repetition)
    // TODO: Store animations in seperate "state" lists for performance
    // TODO: Revise how transitions are used (currently state takes priority, IdleToRun can be called from both Idle and Run - should only be called from Idle)
    // TODO: Start and stop motion matching based on movement (and events)
    // TODO: Convert system to Unity DOTS - can only take NativeArrays<float3>
    
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
    private AnimationClip[] allClips;
    public HumanBodyBones[] joints;
    public AnimContainer animContainer; // put ref to chosen animation container scriptable object

    [SerializeField] private string[] states;

    // --- Variables 
    public bool _preProcess, _playAnimationMode;
    public int pointsPerTrajectory = 4, framesBetweenTrajectoryPoints = 10;
    [SerializeField] private bool _isMotionMatching, _isIdling;
    [SerializeField] private int queryRateInFrames = 10, candidatesPerMisc = 10;
    [Tooltip("The time in frames that it takes for the animaton layers to blend.")]
    [SerializeField] private float animLayerWeightChange = 50.0f;

    private AnimationClip currentClip;
    private int currentFrame, currentID;
    [SerializeField] private float animationFrameRate = 50.0f;

    // --- Weights
    [Range(0, 1)] public float weightRootVel = 1.0f, weightLFootVel = 1.0f, weightRFootVel = 1.0f, weightNeckVel = 1.0f,
	    weightNeckPos = 1.0f, weightFeetPos = 1.0f, weightTrajPoints = 1.0f, weightTrajForwards = 1.0f;

    // --- Debugstuff
    private int animIterator = -1;
    private IEnumerator currentEnumerator;

    private void Awake() // Load animation data
    {
        movement = GetComponent<Movement>();
	    animator = GetComponent<Animator>();
        preProcessing = new PreProcessing();

#if UNITY_EDITOR
        if (_preProcess)
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

            preProcessing.Preprocess(allClips, joints, gameObject, animator, animationFrameRate);
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

        for (int i = 0; i < states.Length; i++)
        {
            states[i] = states[i].ToLower();
        }
        
        _trajCandidatesRef = new List<FeatureVector>();
        _trajPossibleCandidatesRef = new List<FeatureVector>();
    }

    private void Start()
    {
        if (!_playAnimationMode)
        {
            movement.attack += DetectAttack;


            UpdateAnimation(0, 0);
            StartCoroutine(MotionMatch());
        }
    }

    private void DetectAttack()
    {
        animator.SetTrigger("Push");
    }

    public void ChangeLayerWeight(int layerIndex, int desiredWeight)
    {
        StopAllCoroutines();
        StartCoroutine(MotionMatch());
        StartCoroutine(ChangeLayerWeightOverTime(layerIndex, desiredWeight));
    }

    private IEnumerator ChangeLayerWeightOverTime(int layerIndex, int desiredWeight)
    {
        float counter = 0.0f;
        while (counter <= animLayerWeightChange)
        {
            animator.SetLayerWeight(layerIndex, Mathf.Lerp(animator.GetLayerWeight(layerIndex), desiredWeight, counter / animLayerWeightChange));
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            counter++;
        }
    }

    private void FixedUpdate()
    {
        if (!_playAnimationMode)
        {
            if (movement.isJumping)
            {
                Debug.Log("Motion matching detected player is jumping or falling!");
                StopCoroutine(MotionMatch());
                animator.Play("MotionMatching");
                animator.SetTrigger("Jump");
            }
            else if (!_isMotionMatching && !animator.GetCurrentAnimatorStateInfo(1).IsName("Pushing"))
		    {
                Debug.Log("Starting motionmatching");
		    }
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
            {
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
		//Debug.Log("Updating to animation " + currentClip.name + " to frame " + frame + " with ID " + id + " from id " + currentID + " of frame " + currentFrame);
        animator.CrossFadeInFixedTime(currentClip.name, 0.3f, 0, frame / animationFrameRate); // 0.3f was recommended by Magnus
        currentID = id;
        currentFrame = frame;
    }

    private IEnumerator MotionMatch()
    {
	    _isMotionMatching = true;
	    while (_isMotionMatching)
	    {
		    currentID += queryRateInFrames;
            List<FeatureVector> candidates = TrajectoryMatching(movement.GetMovementTrajectory(), ref _trajCandidatesRef, ref _trajPossibleCandidatesRef);
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
            if (!TagChecker(featureVectors[i].GetClipName(), movement.moveState.value))
            {
                continue;
            }
            if ((featureVectors[i].GetID() > currentID ||  featureVectors[i].GetID() < currentID - queryRateInFrames * 2) &&
                 featureVectors[i].GetFrame() + queryRateInFrames <=  featureVectors[i].GetFrameCountForID())
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

        for (int i = 0; i < possibleCandidates.Count; i++)
        {
            if (possibleCandidates[i] != null)
                candidates.Add(possibleCandidates[i]);
        }
        return candidates;
    }

    private int PoseMatching(List<FeatureVector> candidates)
    {
        int bestId = -1;
        float currentDif = float.MaxValue;
        foreach (var candidate in candidates)
        {
            float velDif = featureVectors[currentID].GetPose().ComparePoses(candidate.GetPose(), transform.worldToLocalMatrix.inverse, weightRootVel, weightLFootVel, weightRFootVel, weightNeckVel);
            float feetPosDif = featureVectors[currentID].GetPose().GetJointDistance(candidate.GetPose(), transform.worldToLocalMatrix.inverse, weightFeetPos, weightNeckPos);
            float candidateDif = velDif + feetPosDif; // TODO: Look at joint distance for idle
            if (candidateDif < currentDif)
            {
				//Debug.Log("Candidate ID " + candidate.GetID() + " diff: " + candidateDif + " < " + " Current ID " + bestId + " diff:" + currentDif + "\nVelocity dif was " + velDif + " and feetPos dif was " + feetPosDif);
                bestId = candidate.GetID();
                currentDif = candidateDif;
            }
        }
		return bestId;
    }
    private bool TagChecker(string candidateName, int stateNumber)
    {
	    if (candidateName.ToLower().Contains(states[stateNumber]))
		    return true;
        return false;
    }
    public List<FeatureVector> GetFeatureVectors()
    {
	    return featureVectors;
    }
}