using UnityEngine;

public class FeatureVector
{
	private readonly int id, frame;
	private int allFrames;
    private readonly string clipName;
    private readonly MMPose pose;
    private readonly Trajectory trajectory;

    public FeatureVector(MMPose pose, Trajectory trajectory, int id, string clipName, int frame)
    {
        this.pose = pose;
        this.trajectory = trajectory;
        this.id = id;
        this.clipName = clipName;
        this.frame = frame;
    }

    public void SetFrameCount(int frameCountForID)
    {
	    allFrames = frameCountForID;
    }

    public MMPose GetPose()
    {
        return pose;
    }
    public Trajectory GetTrajectory()
    {
        return trajectory;
    }
    public int GetID()
    {
        return id;
    }
    public string GetClipName()
    {
        return clipName;
    }
    public int GetFrame()
    {
        return frame;
    }
    public int GetFrameCountForID()
    {
	    return allFrames;
    }
    public Trajectory CreateTrajectory(TrajectoryPoint pointAtNextStep, int i) // TODO: Currently redundant, remove or refactor
    {
	    if (i == 0) // We check for index, since we do not want to override the initial trajectory point of the id.
	    {
		    if (trajectory.GetTrajectoryPoints()[0] == null) // This statement should never be true, if it is the instantiation of the trajectories is incorrect
				Debug.Log("Trajectory with ID: " + id + " is missing it's first component!");
	    }
	    else if (pointAtNextStep != null)
	    {
		    TrajectoryPoint tempPoint = pointAtNextStep;
		    if (trajectory.GetTrajectoryPoints()[i] == null || trajectory.GetTrajectoryPoints()[i].GetPoint() == Vector3.zero)
			    trajectory.GetTrajectoryPoints()[i] = tempPoint;
        }
	    else
		    Debug.Log("When trying to populate Trajectory of ID: " + id + " the Point at next step, with index " + i + " is null");
        return trajectory;
    }
}
