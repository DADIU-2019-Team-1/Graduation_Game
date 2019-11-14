using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPoint
{
    public Vector3 point, forward;
    private const float angleFactor = 0.1f, distFactor = 1.0f;
    public TrajectoryPoint()
    {
        point = Vector3.zero;
        forward = Vector3.zero;
    }
    public TrajectoryPoint(Vector3 _point, Vector3 _forward)
    {
        point = _point;
        forward = _forward;
    }
    public Vector3 GetPoint()
    {
        return point;
    }
    public Vector3 GetForward()
    {
        return forward;
    }
    public float GetDiffWithWeights(TrajectoryPoint otherPoint, Matrix4x4 newSpace, float pointWeight, float forwardWeight)
    {
        float diff = 0;
        diff += Vector3.Distance(newSpace.MultiplyPoint3x4(GetPoint()), otherPoint.GetPoint()) * distFactor * pointWeight;
        diff += Vector3.Angle(newSpace.MultiplyPoint3x4(GetForward()), otherPoint.GetForward()) * angleFactor * forwardWeight;
        return diff;
    }
}
