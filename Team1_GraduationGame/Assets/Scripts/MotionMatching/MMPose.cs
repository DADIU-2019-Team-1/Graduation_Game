using UnityEngine;

public class MMPose
{
    private readonly Vector3 rootPos, lFootPos, rFootPos, neckPos,
        rootVel, lFootVel, rFootVel, neckVel;
    public MMPose(Vector3 rootPos, Vector3 lFootPos, Vector3 rFootPos, Vector3 neckPos, Vector3 rootVel, Vector3 lFootVel, Vector3 rFootVel, Vector3 neckVel)
    {
        this.rootPos = rootPos;
        this.lFootPos = lFootPos;
        this.rFootPos = rFootPos;
        this.neckPos = neckPos;
        this.rootVel = rootVel;
        this.lFootVel = lFootVel;
        this.rFootVel = rFootVel;
        this.neckVel = neckVel;
    }

    public Vector3 GetRootPos()
    {
        return rootPos;
    }
    public Vector3 GetLeftFootPos()
    {
        return lFootPos;
    }
    public Vector3 GetRightFootPos()
    {
        return rFootPos;
    }
    public Vector3 GetNeckPos()
    {
        return neckPos;
    }

    public Vector3 GetRootVelocity()
    {
        return rootVel;
    }
    public Vector3 GetLeftFootVelocity()
    {
        return lFootVel;
    }
    public Vector3 GetRightFootVelocity()
    {
        return rFootVel;
    }
    public Vector3 GetNeckVelocity()
    {
        return neckVel;
    }

    public float GetJointDistance(MMPose otherPose, Matrix4x4 newSpace, float feetWeight, float neckWeight)
    {
        float distance = 0;
        distance += Vector3.Distance(newSpace.MultiplyPoint3x4(GetLeftFootPos()), newSpace.MultiplyPoint3x4(otherPose.GetLeftFootPos())) * feetWeight;
        distance += Vector3.Distance(newSpace.MultiplyPoint3x4(GetRightFootPos()), newSpace.MultiplyPoint3x4(otherPose.GetRightFootPos())) * feetWeight;
        distance += Vector3.Distance(newSpace.MultiplyPoint3x4(GetNeckPos()), newSpace.MultiplyPoint3x4(otherPose.GetNeckPos())) * neckWeight;
        return distance;
    }
    public float ComparePoses(MMPose candidatePose, Matrix4x4 newSpace, float rootVelWeight, float lFootVelWeight, float rFootVelWeight, float neckVelWeight)
    {
        float difference = 0;
        difference += Vector3.Distance(newSpace.MultiplyPoint3x4(GetRootVelocity()) * rootVelWeight,
	        newSpace.MultiplyPoint3x4(candidatePose.GetRootVelocity()) * rootVelWeight);
        difference += Vector3.Distance(newSpace.MultiplyPoint3x4(GetLeftFootVelocity()) * lFootVelWeight,
            newSpace.MultiplyPoint3x4(candidatePose.GetLeftFootVelocity()) * lFootVelWeight);
        difference += Vector3.Distance(newSpace.MultiplyPoint3x4(GetRightFootVelocity()) * rFootVelWeight,
            newSpace.MultiplyPoint3x4(candidatePose.GetRightFootVelocity()) * rFootVelWeight);
        difference += Vector3.Distance(newSpace.MultiplyPoint3x4(GetNeckVelocity()) * neckVelWeight,
	        newSpace.MultiplyPoint3x4(candidatePose.GetNeckVelocity()) * neckVelWeight);
        return difference;
    }
}
