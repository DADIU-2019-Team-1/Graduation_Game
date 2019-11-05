using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory
{
    private TrajectoryPoint[] trajectoryPoints;
    private TrajectoryPoint rootPoint;
    private Quaternion rootQ;
    public Trajectory(TrajectoryPoint[] _trajectoryPoints)
    {
        trajectoryPoints = _trajectoryPoints;
    }
    public Trajectory(TrajectoryPoint _trajectoryPoint, Quaternion _rootQ)
    {
	    rootPoint = _trajectoryPoint;
	    rootQ = _rootQ;
    }

    public TrajectoryPoint[] GetTrajectoryPoints()
    {
        return trajectoryPoints;
    }
    public TrajectoryPoint GetRootPoint()
    {
	    return rootPoint;
    }
    public Quaternion GetRotation()
    {
	    return rootQ;
    }
    public float CompareTrajectories(Trajectory otherTrajectory, Matrix4x4 newSpace, float pointWeight, float forwardWeight)
    {
        float dist = 0;
        for (int i = 0; i < trajectoryPoints.Length; i++)
        {
            dist += trajectoryPoints[i].GetDiffWithWeights(otherTrajectory.trajectoryPoints[i], newSpace, pointWeight, forwardWeight);
        }
        // Debug.Log("Compare Trajectories distance is: " + dist);
        return dist;
    }
}
