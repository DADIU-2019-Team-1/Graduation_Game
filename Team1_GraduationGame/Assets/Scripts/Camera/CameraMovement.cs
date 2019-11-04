// Code owner: Jannik Neerdal
using UnityEngine;
using System.Collections.Generic;
using Cinemachine;

public class CameraMovement : MonoBehaviour
{
    // --- References
    [Tooltip("If Player Target is not set, object with tag \"Player\" will be used instead.")]
    public Transform player;
    [Tooltip("If Camera Rail is not set, object with tag \"RailCamera\" will be used instead.")]
    public Transform camRail;

    [Tooltip("If the Camera Track is not set, object with the script \"Cinemachine Smooth Path\" will be used instead")]
    public GameObject track;
    private CinemachineSmoothPath _trackPath;
    private CameraLook _cameraLook;
    private Camera thisCam;

    // --- Inspector
    [Tooltip("Use to create an array of tags that will be used as focus points. Each object with a tag in this array will be counted as a focus point, if within the focus range.")]
    [TagSelector] [SerializeField] private string[] tagsToFocus;
    [Tooltip("This value is used to determine the height when the target is far away.")] 
    [SerializeField] private FloatReference heightDistanceFactor;
    [Tooltip("A higher value makes the camera LookAt more aggressive.")]
    [SerializeField] private FloatReference camLookSpeed;
    [Tooltip("A lower value makes the camera move to the desired position faster.")]
    [SerializeField] private FloatReference camMoveTime;
    [Tooltip("Range from player .")]
    [SerializeField] private FloatReference focusRange;
    [Tooltip("Determines how long it should take for the FOV to update. 1 is instant.")]
    [SerializeField] [Range(0.01f, 1.0f)] private float fovUpdateTime = 0.05f;

    // --- Private
    private List<GameObject> focusObjects;
    private Quaternion targetRotation;
    private Vector3 camMovement, lookPosition;
    private float heightIncrease, trackX, startingFOV, currentFOV;
    private bool _endOfRail;
    [HideInInspector] public int previousTrackIndex, nextTrackIndex;

    void Start()
    {
        if (GetComponent<Camera>() != null)
            thisCam = GetComponent<Camera>();
        else
            Debug.LogError("This script is not attached to an object with a Camera!");
        startingFOV = thisCam.fieldOfView;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        if (track == null)
            track = FindObjectOfType<CinemachineSmoothPath>().gameObject;
        _trackPath = track.GetComponent<CinemachineSmoothPath>();
        _cameraLook = track.GetComponent<CameraLook>();

        if (camRail == null)
            camRail = GameObject.FindGameObjectWithTag("RailCamera").transform;

        focusObjects = new List<GameObject>();
        for (int i = 0; i < tagsToFocus.Length; i++)
            focusObjects.AddRange(GameObject.FindGameObjectsWithTag(tagsToFocus[i]));

        // Init position and rotation
        heightIncrease = Vector3.Distance(player.position, new Vector3(player.position.x, camRail.position.y, camRail.position.z)) * heightDistanceFactor.value;
        transform.LookAt(player);

        trackX = _trackPath.gameObject.transform.position.x;
        if (player.position.x >= (_trackPath.m_Waypoints[0].position.x + trackX) && 
            player.position.x <= _trackPath.m_Waypoints[_trackPath.m_Waypoints.Length - 1].position.x + trackX)
            {
                transform.position = new Vector3(player.position.x, camRail.position.y + heightIncrease, camRail.position.z);
            }
        else
        {
            transform.position = new Vector3(camRail.position.x, camRail.position.y + heightIncrease, camRail.position.z);
        }

    }

    void LateUpdate()
    {
        float diff = float.MaxValue;
        int bestIndex = -1;
        for (int i = 0; i < _trackPath.m_Waypoints.Length; i++)
        {
            if (Mathf.Abs(camRail.position.x - (_trackPath.m_Waypoints[i].position.x + trackX)) < diff)
            {
                diff = Mathf.Abs(camRail.position.x - (_trackPath.m_Waypoints[i].position.x + trackX));
                bestIndex = i;
            }
        }
        if (camRail.position.x < _trackPath.m_Waypoints[bestIndex].position.x + trackX || camRail.position.x == _trackPath.m_Waypoints[_trackPath.m_Waypoints.Length - 1].position.x + trackX)
        {
            nextTrackIndex = bestIndex;
            previousTrackIndex = bestIndex - 1;
        }
        else if (camRail.position.x > _trackPath.m_Waypoints[bestIndex].position.x + trackX || camRail.position.x == _trackPath.m_Waypoints[0].position.x + trackX)
        {
            nextTrackIndex = bestIndex + 1;
            previousTrackIndex = bestIndex;
        }

        heightIncrease = Vector3.Distance(player.position, new Vector3(player.position.x, camRail.position.y, camRail.position.z)) * heightDistanceFactor.value;
        // Check if camera has passed the end of the track
        if (!_endOfRail)
        {
            // Position update
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(player.position.x, camRail.position.y + heightIncrease, camRail.position.z),
                ref camMovement, camMoveTime.value * Time.deltaTime);
            if (transform.position.x < (_trackPath.m_Waypoints[0].position.x + trackX) || transform.position.x > _trackPath.m_Waypoints[_trackPath.m_Waypoints.Length - 1].position.x + trackX)
                _endOfRail = true;
        }
        else
        {
            // Y and Z update
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(transform.position.x, camRail.position.y + heightIncrease, camRail.position.z),
                ref camMovement, camMoveTime.value * Time.deltaTime);
            if (player.position.x >= (_trackPath.m_Waypoints[0].position.x + trackX) && player.position.x <= _trackPath.m_Waypoints[_trackPath.m_Waypoints.Length - 1].position.x + trackX)
                _endOfRail = false;
        }

        // Rotation update
        lookPosition = CalculateLookPosition(player.position, _cameraLook.camTarget, focusRange.value, focusObjects);
        targetRotation = (lookPosition - transform.position != Vector3.zero)
            ? Quaternion.LookRotation(lookPosition - transform.position) : Quaternion.identity;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, camLookSpeed.value * Time.deltaTime);

        // FOV Update
        thisCam.fieldOfView = Mathf.Lerp(thisCam.fieldOfView, startingFOV + Mathf.Abs(_cameraLook.offsetTrack[nextTrackIndex].GetFOV() - _cameraLook.offsetTrack[previousTrackIndex].GetFOV()) / 2, fovUpdateTime);
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
            if (Vector3.Distance(systemCenter, objects[i].transform.position) <= systemRadius)
            {
                float weight = (systemRadius - Vector3.Distance(systemCenter, objects[i].transform.position)) / systemRadius;
                totalWeight += weight;

                focusWithWeights += objects[i].transform.position * weight;
            }
        }
        return focusWithWeights / totalWeight;
    }
}
