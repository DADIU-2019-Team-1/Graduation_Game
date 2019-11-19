using Unity.Mathematics;
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
}
