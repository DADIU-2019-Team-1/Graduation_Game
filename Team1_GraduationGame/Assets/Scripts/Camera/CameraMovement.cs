// Code owner: Jannik Neerdal - Optimized
using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class CameraMovement : MonoBehaviour
{
    // --- References
    [Tooltip("If Player Target is not set, object with tag \"Player\" will be used instead.")]
    public Transform player;
    [Tooltip("If Camera Rail is not set, object with tag \"RailCamera\" will be used instead.")]
    public Transform railCam;

    [Tooltip("If the Camera Track is not set, object with the script \"Cinemachine Smooth Path\" will be used instead")]
    public GameObject track;
    private CinemachineSmoothPath _trackPath;
    private CameraLook _cameraLook;
    [HideInInspector] public Camera thisCam;

    // --- Inspector
    [Tooltip("Use to create an array of tags that will be used as focus points. Each object with a tag in this array will be counted as a focus point, if within the focus range.")]
    [TagSelector] [SerializeField] private string[] tagsToFocus;
    [Tooltip("This value is used to determine the height when the target is far away.")]
    [SerializeField] private FloatReference heightDistanceFactor;
    [Tooltip("A higher value makes the camera LookAt more aggressive.")]
    [SerializeField] private FloatReference camLookSpeed;
    [Tooltip("A lower value makes the camera move to the desired position faster.")]
    [SerializeField] private FloatReference camMoveTime;
    [Tooltip("Range from player before focus objects are weighted.")]
    [SerializeField] private FloatReference focusRange;

    // --- Hidden
    private List<GameObject> focusObjects = new List<GameObject>();
    private Quaternion _targetRotation;
    private Vector3 _camMovement,
        _lookPosition, 
        _currentPosition;
    private float _heightIncrease;
    private bool _endOfRail;
    [HideInInspector] public int previousTrackIndex, 
        nextTrackIndex;
    [HideInInspector] public float trackX;

    void Start()
    {
        // Assign reference variables if they have not been assigned in the inspector
        if (GetComponent<Camera>() != null)
            thisCam = GetComponent<Camera>();
        else
            Debug.LogError("This script is not attached to an object with a Camera!");

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (track == null)
            track = FindObjectOfType<CinemachineSmoothPath>().gameObject;
        _trackPath = track.GetComponent<CinemachineSmoothPath>();
        _cameraLook = track.GetComponent<CameraLook>();

        if (railCam == null)
            railCam = GameObject.FindGameObjectWithTag("RailCamera").transform;

        // Add all focus objects to a list, based on tags selected in the inspector (if any)
        for (int i = 0; i < tagsToFocus.Length; i++)
            focusObjects.AddRange(GameObject.FindGameObjectsWithTag(tagsToFocus[i]));

        // Init position and rotation
        transform.LookAt(player);
        _heightIncrease = Vector3.Distance(player.position, new Vector3(player.position.x, railCam.position.y, railCam.position.z)) * heightDistanceFactor.value;

        trackX = _trackPath.gameObject.transform.position.x;
        if (player.position.x >= (_trackPath.m_Waypoints[0].position.x + trackX) && player.position.x <= _trackPath.m_Waypoints[_trackPath.m_Waypoints.Length - 1].position.x + trackX)
        {
            transform.position = new Vector3(player.position.x, railCam.position.y + _heightIncrease, railCam.position.z);
        }
        else
        {
            transform.position = new Vector3(railCam.position.x, railCam.position.y + _heightIncrease, railCam.position.z);
        }
    }

    void FixedUpdate()
    {
        // Find the closest point of the track to the camera and store it's index
        float diff = float.MaxValue;
        int bestIndex = -1;
        for (int i = 0; i < _trackPath.m_Waypoints.Length; i++) // For further optimization, this could be handled in a job
        {
            if (Mathf.Abs(railCam.position.x - (_trackPath.m_Waypoints[i].position.x + trackX)) < diff)
            {
                diff = Mathf.Abs(railCam.position.x - (_trackPath.m_Waypoints[i].position.x + trackX));
                bestIndex = i;
            }
        }

        // Determine the the next and previous track indices based on camera position (which is the two points the camera is in between)
        if (railCam.position.x < _trackPath.m_Waypoints[bestIndex].position.x + trackX || railCam.position.x == _trackPath.m_Waypoints[_trackPath.m_Waypoints.Length - 1].position.x + trackX)
        {
            nextTrackIndex = bestIndex;
            previousTrackIndex = bestIndex - 1;
        }
        else if (railCam.position.x > _trackPath.m_Waypoints[bestIndex].position.x + trackX || railCam.position.x == _trackPath.m_Waypoints[0].position.x + trackX)
        {
            nextTrackIndex = bestIndex + 1;
            previousTrackIndex = bestIndex;
        }

        _heightIncrease = Vector3.Distance(player.position, new Vector3(player.position.x, railCam.position.y, railCam.position.z)) * heightDistanceFactor.value;
        
        // Check if camera has passed the end of the track
        if (!_endOfRail)
        {
            // Position update
            _currentPosition = Vector3.SmoothDamp(_currentPosition, new Vector3(player.position.x, railCam.position.y + _heightIncrease, railCam.position.z) + _cameraLook.camPosOffset,
                ref _camMovement, camMoveTime.value * Time.deltaTime);
            if (Vector3.SqrMagnitude(transform.position - _currentPosition) >= 0.001f) // Only update the transform if there is a change in the position
            {
                transform.position = _currentPosition;
                if (transform.position.x < (_trackPath.m_Waypoints[0].position.x + trackX) || transform.position.x > _trackPath.m_Waypoints[_trackPath.m_Waypoints.Length - 1].position.x + trackX)
                    _endOfRail = true;
            }
        }
        else
        {
            // Only Y and Z update
            _currentPosition = Vector3.SmoothDamp(_currentPosition, new Vector3(transform.position.x, railCam.position.y + _heightIncrease, railCam.position.z),
                ref _camMovement, camMoveTime.value * Time.deltaTime);
            if (Vector3.SqrMagnitude(transform.position - _currentPosition) >= 0.001f) // Only update the transform if there is a change in the position
            {
                transform.position = _currentPosition;
                if (player.position.x + _cameraLook.camPosOffset.x >= (_trackPath.m_Waypoints[0].position.x + trackX) && player.position.x + _cameraLook.camPosOffset.x <= _trackPath.m_Waypoints[_trackPath.m_Waypoints.Length - 1].position.x + trackX)
                    _endOfRail = false;
            }
        }
    }

    void LateUpdate()
    {
        // Rotation update - Should be after position update to avoid jitter
        _lookPosition = CalculateLookPosition(player.position, _cameraLook.camTarget, focusRange.value, focusObjects);
        _targetRotation = _lookPosition - transform.position != Vector3.zero ? Quaternion.LookRotation(_lookPosition - transform.position) : transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, camLookSpeed.value * Time.deltaTime);
    }

    /// <summary>
    /// Returns a position which represent the center of mass of a system, where the mass is a value from 0-1,
    /// based on the proximity to the target. Takes a main target which serves as the focus of the system, a list of objects
    /// in the system, and an empty reference list of floats, used to calculate the point weights.
    /// </summary>
    /// <param name="systemCenter">The target for the system to be centered around.</param>
    /// <param name="currentTarget">The current look target, which should be weighted 1 regardless. Can be the same as the system center.</param>
    /// <param name="systemRadius">The radius around the target for the system to be created around.</param>
    /// <param name="objects">The objects that, if within the radius of the target, are included in the system.</param>
    /// <returns></returns>
    private Vector3 CalculateLookPosition(Vector3 systemCenter, Vector3 currentTarget, float systemRadius, List<GameObject> objects)
    {
        Vector3 focusWithWeights = currentTarget;
        float totalWeight = 1; // Starts at 1 because the currentTarget has a weight of 1
        for (int i = 0; i < objects.Count; i++)
        {
            if (math.distancesq(systemCenter, objects[i].transform.position) <= math.pow(systemRadius,2))
            {
                float weight = math.clamp((math.pow(systemRadius, 2) - math.distancesq(systemCenter, objects[i].transform.position)) / math.pow(systemRadius, 2), 0, float.MaxValue);
                totalWeight += weight;

                focusWithWeights += objects[i].transform.position * weight;
            }
        }
        return focusWithWeights / totalWeight;
    }
}
