using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTest : MonoBehaviour
{

    // For every variable used, make a scriptableObject with the type. So make a scriptable object script that takes a single float,
    // For every data type, have a TypeReference for the scriptableObject. Create new SOs through asset menu for each variable here. 


    // GetMovementTrajectoryCharacterSpace function. Currently movement is in worldspace, and animations in character space,
    // We need a function to change that, so the movements are properly applied to the character in the right space. 
    
    // --- References
    private MotionMatching mm;

	// --- Public
    [Tooltip("In degrees")] public float rotateToMoveThreshold = 15f;
    public float rotateSpeed = 2.0f;
    public FloatReference sneakSpeed, walkSpeed, runSpeed, lerpTime, movementSpeed, movementMultiplier;

    // --- Private 
    private Vector3 prevPos, prevRot, goalPos, slerpRotation;

    private Vector3 inputDir, charInputDir, desiredDir, charForward; 
    [SerializeField]
    private float speed, angSpeed, rotationValue;

	private float speedSmoothVelocity, turnSmoothVelocity;
    private Matrix4x4 charSpace;
    private Quaternion lookRotation;
    private Vector3 translation;

    private string movementType;

    private void Awake()
    {
	    mm = GetComponent<MotionMatching>();
    }

    private void Start()
    {
	    charSpace = new Matrix4x4();
	    lookRotation = Quaternion.identity;
		slerpRotation = Vector3.zero;
    }
	
    private void FixedUpdate()
    {
	    UpdateSpeed();
        UpdateAngularSpeed();
        switch (movementType) {
            case "wasd":
                KeyBoardMove();
                break;

            case "joystick":
                ClickAndDrag();
                if(transform.position != goalPos) {
                    goalPos = transform.position;
                }
                
                break;

            case "moveToPoint":
                MoveToMouse();
                break;

            default:
                movementType = "wasd";
                //Debug.Log("Unknown movetype");
                break;
        }

        if(Input.GetKeyDown("o")) {
            ChangeMovement();
        }
    }

    private void OnDrawGizmos()
    {
	    if (Application.isPlaying)
	    {
		    Matrix4x4 invCharSpace = transform.worldToLocalMatrix.inverse;
		    Matrix4x4 charSpace = transform.localToWorldMatrix;
		    Matrix4x4 newSpace = new Matrix4x4();
		    newSpace.SetTRS(transform.position, Quaternion.identity, transform.lossyScale);

            Gizmos.color = Color.red; // Movement Trajectory
            //for (int i = 1; i < GetMovementTrajectory().GetTrajectoryPoints().Length; i++) // Gizmos for movement
            //{
            //    // Position
            //    Gizmos.DrawWireSphere(GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(), 0.2f);
            //    Gizmos.DrawLine(i != 0 ? GetMovementTrajectory().GetTrajectoryPoints()[i - 1].GetPoint() : transform.position,
            //     GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint());

            //    // Forward
            //    Gizmos.DrawLine(GetMovementTrajectory().GetTrajectoryPoints()[i].GetPoint(),
            //     GetMovementTrajectory().GetTrajectoryPoints()[i].GetForward());
            //}

            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, inputDir);
            Gizmos.DrawWireSphere(inputDir, 0.15f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Vector3.zero, charInputDir);
            Gizmos.DrawWireSphere(charInputDir, 0.15f);

            Gizmos.color = Color.magenta;
			Gizmos.DrawLine(Vector3.zero, desiredDir);
			Gizmos.DrawWireSphere(desiredDir, 0.15f);

            //Gizmos.color = Color.cyan;
            //Gizmos.DrawLine(Vector3.zero, charDesiredDir);
            //Gizmos.DrawWireSphere(charDesiredDir, 0.15f);

            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(Vector3.zero, transform.forward);
            //Gizmos.DrawWireSphere(transform.forward, 0.15f);

            //Gizmos.color = Color.white;
            //Gizmos.DrawLine(Vector3.zero, Vector3.zero + lookRotation * Vector3.forward * speed);
            //Gizmos.DrawWireSphere(Vector3.zero + lookRotation * Vector3.forward * speed, 0.15f);

            //Gizmos.color = Color.blue;
            //Quaternion lookRotation = charInputDir != Vector3.zero ? Quaternion.LookRotation(charInputDir) : transform.rotation;
            //         Gizmos.DrawLine(Vector3.zero, Quaternion.Slerp(transform.rotation, lookRotation, 1.0f) * Vector3.forward);
            //Gizmos.DrawWireSphere(Quaternion.Slerp(transform.rotation, lookRotation, 1.0f) * Vector3.forward, 0.15f);
        }
    }

    private void UpdateSpeed()
    {
	    //speed = (transform.position - prevPos).magnitude / Time.fixedDeltaTime;
	    //speed = speed / 3.0f;
	    //prevPos = transform.position;
    }

    private void UpdateAngularSpeed()
    {
        //angSpeed = (transform.rotation.eulerAngles - prevRot).magnitude / Time.fixedDeltaTime;
        //prevRot = transform.rotation.eulerAngles;
    }

    public float GetSpeed()
    {
	    return speed;
    }

    public float GetAngularSpeed()
    {
        return angSpeed;
    }

    public Trajectory GetMovementTrajectory()
    {
        TrajectoryPoint[] points = new TrajectoryPoint[mm.pointsPerTrajectory];

        for (int i = 0; i < points.Length; i++)
		{
			if (i > 0)
			{
				//Vector3 tempPos = points[i - 1].GetPoint() + Quaternion.Slerp(transform.rotation, lookRotation, (float) (i + 1) / points.Length) * Vector3.forward * Mathf.Clamp(speed, 0.1f, 1.0f);
				Vector3 tempPos = points[i - 1].GetPoint() + inputDir * speed;

                Vector3 tempForward = tempPos + Quaternion.Slerp(transform.rotation, lookRotation, 
	                                      (float)(i + 1) / points.Length) * Vector3.forward  * rotationValue;

                points[i] = new TrajectoryPoint(tempPos, tempForward);
			}
			else
				points[i] = new TrajectoryPoint(transform.position, transform.position +  transform.forward);
		}
		return new Trajectory(points);
    }

    public Vector3 GetMovementVelocity()
    {
	    return (transform.position - prevPos) / Time.fixedDeltaTime;
    }

    public void KeyBoardMove()
    {
		// Input
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        inputDir = Clamp(input);

		// Change speeds based on movement style (Sneak -> Walking -> Running) // TODO: Add sneak
        bool running = Input.GetKey(KeyCode.LeftShift);
        bool sneaking = Input.GetKey(KeyCode.LeftControl);
        float targetSpeed;
        if (running)
        {
	        targetSpeed = runSpeed.value * inputDir.magnitude;
	        rotationValue = rotateSpeed * 2.0f * inputDir.magnitude; // TODO: Rename rotationValue and add variables to different rotation speeds
        }
		else if (sneaking)
        {
	        targetSpeed = sneakSpeed.value * inputDir.magnitude;
	        rotationValue = rotateSpeed * 0.5f * inputDir.magnitude;
        }
        else
        {
	        targetSpeed = walkSpeed.value * inputDir.magnitude;
	        rotationValue = rotateSpeed * inputDir.magnitude;
        }
        speed = Mathf.SmoothDamp(speed, targetSpeed, ref speedSmoothVelocity, movementSpeed.value);

        // Rotation
        lookRotation = inputDir != Vector3.zero ? Quaternion.LookRotation(inputDir) : transform.rotation; // Avoid LookRotation zero-errors 
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationValue * Time.fixedDeltaTime);

		// Translation
	    translation = inputDir * speed * Time.fixedDeltaTime;
        transform.Translate(translation, Space.World);
    }
    public void MoveToMouse() {
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit)) {
                goalPos = hit.point;
            }
        }

        if(Vector3.Distance(transform.position, goalPos) > 0.2f) {
            Quaternion rotation = goalPos - transform.position != Vector3.zero
                ? Quaternion.LookRotation(goalPos - transform.position) : Quaternion.identity; // Shorthand if : else
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * speed + 0.1f);
            prevPos = transform.position;
            prevRot = transform.rotation.eulerAngles;

            if (CheckRotateToMove(rotation))
                transform.position = Vector3.Lerp(transform.position, goalPos, (movementSpeed.value * movementMultiplier.value) * Time.fixedDeltaTime);
        }
    }

    public void ClickAndDrag() {
        if(Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit)) {
                goalPos = new Vector3(hit.point.x, 0.0f, hit.point.z);

                Quaternion rotation = goalPos - transform.position != Vector3.zero
                    ? Quaternion.LookRotation(goalPos - transform.position) : Quaternion.identity; // Shorthand if : else

                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.fixedDeltaTime * speed + 0.1f);


                if (CheckRotateToMove(rotation))
                    transform.position = Vector3.Lerp(transform.position, goalPos, (movementSpeed.value * movementMultiplier.value ) * Time.fixedDeltaTime);
            }
        }
    }

    public void ChangeMovement() {
        if(movementType == "wasd") {
            movementType = "joystick";
            Debug.Log("Movement type is: " + movementType);
        }

        else if(movementType == "joystick") {
            movementType = "moveToPoint";
            Debug.Log("Movement type is: " + movementType);
        }

        else if(movementType == "moveToPoint") {
            movementType = "wasd";
            Debug.Log("Movement type is: " + movementType);
        }
    }

    private bool CheckRotateToMove(Quaternion rotation)
    {
        bool tempBool = false;
        float rotationChecker = transform.rotation.eulerAngles.y;
        float upperBound = (rotationChecker + rotateToMoveThreshold) % 360f, lowerBound = rotationChecker - rotateToMoveThreshold;

        if (rotationChecker - rotateToMoveThreshold < 0)
        {
            rotationChecker += 360;
            lowerBound = rotationChecker;
        } 
        
        if (lowerBound > upperBound)
        {
            if (rotation.eulerAngles.y <= upperBound)
            {
                lowerBound = upperBound - rotateToMoveThreshold * 2;
            }
            else if (rotation.eulerAngles.y >= lowerBound)
            {
                upperBound = lowerBound + rotateToMoveThreshold * 2;
            }
        }

        if (rotation.eulerAngles.y <= upperBound &&
            rotation.eulerAngles.y >= lowerBound)
            tempBool = true;
        
        return tempBool;
    }

	/// <summary>
    /// Returns a Vector3 clamped to a magnitude of 1. Useful for creating dynamic direction vectors.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public Vector3 Clamp(Vector3 vector)
    {
	    if (vector.magnitude >= 1)
		    return vector.normalized;
	    return vector;
    }


    // TODO: Delete this
    #region OldCode
    // Movement along the z axis - Desired dir has to change over time
    //Vector3 translation = Quaternion.Slerp(transform.rotation, lookRotation, movementSpeed.value) * inputDir * movementSpeed.value;
    //Vector3 translation = transform.forward * input.z * movementSpeed.value;
    //Vector3 translation = transform.forward * movementSpeed.value * speed * Time.fixedDeltaTime;
    //      Quaternion lookRotation = desiredDir != Vector3.zero ? Quaternion.LookRotation(charRotSpace.MultiplyPoint3x4(desiredDir)) : transform.rotation; // Avoid LookRotation zero-errors 
    //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 0.1f);

    //      //Matrix4x4 unrotatedTransform = new Matrix4x4();
    //      //unrotatedTransform.SetTRS(transform.position, Quaternion.identity, Vector3.one);
    //      //Vector3 desiredPos = unrotatedTransform.MultiplyPoint3x4(new Vector3(0.0f, 0.0f, Input.GetAxis("Vertical")));
    //      Vector3 desiredPos = transform.worldToLocalMatrix.inverse.MultiplyPoint3x4(new Vector3(0.0f, 0.0f, Input.GetAxis("Vertical")));
    //      desiredDir = desiredPos - transform.position;

    //      if (Input.GetAxis("Vertical") >= 0.1f || Input.GetAxis("Vertical") <= -0.1f)
    //      {
    //       //transform.position = prevPos + transform.forward * Input.GetAxis("Vertical") * movementSpeed.value;
    //       transform.position = transform.position + desiredDir * movementSpeed.value;
    //      }
    //transform.position = prevPos;
    #endregion
}
