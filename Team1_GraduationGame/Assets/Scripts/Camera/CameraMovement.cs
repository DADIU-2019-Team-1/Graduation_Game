using UnityEngine;
using System.Collections.Generic;
using Cinemachine;

public class CameraMovement : MonoBehaviour
{
    // --- References
    [Tooltip("If Camera Target is not set, object with tag \"CamLookAt\" will be used instead. \"Player\" will be used as a fallback.")]
    public Transform camTarget;
    [Tooltip("If Player Target is not set, object with tag \"Player\" will be used instead.")]
    public Transform player;
    [Tooltip("If Camera Rail is not set, object with tag \"RailCamera\" will be used instead.")]
    public Transform camRail;
    [Tooltip("If the Camera Track is not set, object with he script \"Cinemachine Smooth Path\" will be used instead")]
    public CinemachineSmoothPath track;

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

    // --- Private
    private List<GameObject> focusObjects;
    private Quaternion targetRotation;
    private Vector3 camMovement, lookPosition;
    private float heightIncrease, trackX;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        if (camTarget == null)
        {
            camTarget = GameObject.FindGameObjectWithTag("CamLookAt").transform;
            if (camTarget == null)
                camTarget = player;
        }
        if (camRail == null)
            camRail = GameObject.FindGameObjectWithTag("RailCamera").transform;
        if (track == null)
            track = FindObjectOfType<CinemachineSmoothPath>();

        focusObjects = new List<GameObject>();
        for (int i = 0; i < tagsToFocus.Length; i++)
            focusObjects.AddRange(GameObject.FindGameObjectsWithTag(tagsToFocus[i]));

        // Init position and rotation
        heightIncrease = Vector3.Distance(player.position, new Vector3(player.position.x, camRail.position.y, camRail.position.z)) * heightDistanceFactor.value;
        transform.position = new Vector3(player.position.x, camRail.position.y + heightIncrease, camRail.position.z);
        transform.LookAt(player);
    }

    void LateUpdate()
    {
        // Position update
        heightIncrease = Vector3.Distance(player.position, new Vector3(player.position.x, camRail.position.y, camRail.position.z)) * heightDistanceFactor.value;
        lookPosition = CalculateLookPosition(player.position, camTarget.position, focusRange.value, focusObjects);
        
        trackX = track.gameObject.transform.position.x;
        if (transform.position.x >= (track.m_Waypoints[0].position.x + trackX) 
            && transform.position.x <= track.m_Waypoints[track.m_Waypoints.Length - 1].position.x + trackX)
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(player.position.x, camRail.position.y + heightIncrease, camRail.position.z),
            ref camMovement, camMoveTime.value * Time.deltaTime);
        else
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(camRail.position.x, camRail.position.y + heightIncrease, camRail.position.z),
                ref camMovement, camMoveTime.value * Time.deltaTime);

        // Rotation update
        targetRotation = (lookPosition - transform.position != Vector3.zero)
            ? Quaternion.LookRotation(lookPosition - transform.position) : Quaternion.identity;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, camLookSpeed.value * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(lookPosition, 0.5f);
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
