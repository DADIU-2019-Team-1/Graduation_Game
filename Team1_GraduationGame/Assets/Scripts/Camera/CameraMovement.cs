using UnityEngine;
using System.Collections.Generic;
public class CameraMovement : MonoBehaviour
{
    // --- Public
    [Tooltip("If Camera Target is not set, object with tag \"CamLookAt\" will be used instead. \n\"Player\" will be used as a fallback.")]
    public Transform camTarget;
    [Tooltip("If Player Target is not set, object with tag \"Player\" will be used instead.")]
    public Transform player;
    [Tooltip("If Camera Rail is not set, object with tag \"RailCamera\" will be used instead.")]
    public Transform camRail;

    // --- Inspector
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
    [SerializeField] private List<GameObject> focusObjects; // TODO: Remove SerializeField when feature is approved
    [SerializeField] private List<float> focusPointWeights; // Includes the player, so start at 1  // TODO: Remove SerializeField when feature is approved
    private float heightIncrease;
    private Vector3 camMovement, lookPosition;

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

        focusObjects = new List<GameObject>();
        focusPointWeights = new List<float>();
        for (int i = 0; i < tagsToFocus.Length; i++)
            focusObjects.AddRange(GameObject.FindGameObjectsWithTag(tagsToFocus[i]));
    }

    void LateUpdate()
    {
        // Position update
        heightIncrease = Vector3.Distance(player.position, new Vector3(player.position.x, camRail.position.y, camRail.position.z)) * heightDistanceFactor.value;
        lookPosition = CalculateLookPosition(camTarget.position, focusRange.value, focusObjects, ref focusPointWeights);
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(camTarget.position.x, camRail.position.y + heightIncrease, camRail.position.z),
            ref camMovement, camMoveTime.value * Time.deltaTime);

        // Rotation update
        Quaternion targetRotation = (lookPosition - transform.position != Vector3.zero)
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
    /// <param name="target">The target for the system to be created around.</param>
    /// <param name="systemRadius">The radius around the target for the system to be created around.</param>
    /// <param name="objects">The objects that, if within the radius of the target, are included in the system.</param>
    /// <param name="pointWeights">A reference list of floats to hold the weights for each object in the system.</param>
    /// <returns></returns>
    private Vector3 CalculateLookPosition(Vector3 target, float systemRadius, List<GameObject> objects, ref List<float> pointWeights)
    {
        pointWeights.Clear();
        float avgFocusX = target.x;
        float avgFocusY = target.y;
        float avgFocusZ = target.z;
        for (int i = 0; i < objects.Count; i++)
        {
            if (Vector3.Distance(target, objects[i].transform.position) <= systemRadius)
            {
                float weight = (systemRadius - Vector3.Distance(target, objects[i].transform.position)) / systemRadius;
                pointWeights.Add(weight);

                avgFocusX += objects[i].transform.position.x * weight;
                avgFocusY += objects[i].transform.position.y * weight;
                avgFocusZ += objects[i].transform.position.z * weight;
            }
        }

        float totalWeight = 1; // Starts at 1 because the target has a weight of 1

        for (int i = 0; i < pointWeights.Count; i++)
            totalWeight += pointWeights[i];

        return new Vector3(avgFocusX / totalWeight, avgFocusY / totalWeight, avgFocusZ / totalWeight);
    }
}
