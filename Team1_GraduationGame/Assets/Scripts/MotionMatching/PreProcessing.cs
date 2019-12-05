// Code Owner: Jannik Neerdal
using System.Collections.Generic;
using UnityEngine;

namespace Team1_GraduationGame.MotionMatching
{
    public class PreProcessing
    {
        // --- References
        private CSVHandler csvHandler;

        // --- Collections
        private List<string> allClipNames;
        private List<int> allClipFrameCounts, allFrames, allStates;
        private List<MMPose> allPoses;
        private List<TrajectoryPoint> allPoints;

        // --- Variables
        private const float velFactor = 100.0f;
        private bool ignoreRotation;

        public void Preprocess(AnimationClip[] allClips, HumanBodyBones[] joints, GameObject avatar, Animator animator,
            float frameSampleRate, string[] states)
        {
            csvHandler = new CSVHandler();

            allClipNames = new List<string>();
            allClipFrameCounts = new List<int>();
            allFrames = new List<int>();
            allStates = new List<int>();
            allPoses = new List<MMPose>();
            allPoints = new List<TrajectoryPoint>();

            Matrix4x4 startSpace = new Matrix4x4();
            Matrix4x4 charSpace = new Matrix4x4();
            for (int i = 0; i < allClips.Length; i++)
            {
                allClips[i].SampleAnimation(avatar, 0); // First frame of currently sampled animation
                Vector3 startPosForClip = animator.GetBoneTransform(joints[0]).position.GetXZVector3();
                Quaternion startRotForClip = animator.GetBoneTransform(joints[0]).rotation;
                startSpace.SetTRS(startPosForClip, startRotForClip, Vector3.one);

                //Debug.Log("Clip " + i + ": Original Forward " + animator.GetBoneTransform(joints[0]).forward + " | New Forward " + startSpace.inverse.MultiplyVector(animator.GetBoneTransform(joints[0]).forward) + " | Rotation: " + animator.GetBoneTransform(joints[0]).rotation.eulerAngles);

                Vector3 preRootPos = Vector3.zero,
                    preLFootPos = Vector3.zero,
                    preRFootPos = Vector3.zero,
                    preNeckPos = Vector3.zero;
                string lowercaseName = allClips[i].name.ToLower();
                int clipState = -1;
                for (int j = 0; j < states.Length; j++)
                {
                    if (lowercaseName.Contains(states[j]) && !lowercaseName.Contains("from" + states[j]))
                    {
                        clipState = j;
                        break;
                    }
                }
                if (clipState == -1)
                {
                    Debug.Log("During preprocessing, clip " + allClips[i].name + " did not fit into any states, and was therefore not stored as a feature vector!");
                    continue;
                }

                ignoreRotation = lowercaseName.Contains("forward");

                int clipFrameCount = (int) (allClips[i].length * frameSampleRate) - 1;
                for (int j = 0; j < (int) (allClips[i].length * frameSampleRate); j++)
                {
                    allClips[i].SampleAnimation(avatar, j / frameSampleRate);
                    allClipNames.Add(allClips[i].name);
                    allClipFrameCounts.Add(clipFrameCount);
                    allFrames.Add(j);
                    allStates.Add(clipState);

                    Vector3 rootPos = startSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[0]).position.GetXZVector3());
                    if (ignoreRotation) // Ignore x-axis for forward animations to eliminitate bad tracking data
                        rootPos = new Vector3(0.0f, 0.0f, rootPos.z);

                    charSpace.SetTRS(animator.GetBoneTransform(joints[0]).position.GetXZVector3(), animator.GetBoneTransform(joints[0]).rotation, Vector3.one);
                    Vector3 lFootPos = charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[1]).position);
                    Vector3 rFootPos = charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[2]).position);
                    Vector3 neckPos = charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[3]).position);
                    if (j != 0)
                    {
                        allPoses.Add(new MMPose(rootPos, lFootPos, rFootPos, neckPos,
                            CalculateVelocity(rootPos, preRootPos, velFactor),
                            CalculateVelocity(lFootPos, preLFootPos, velFactor),
                            CalculateVelocity(rFootPos, preRFootPos, velFactor),
                            CalculateVelocity(neckPos, preNeckPos, velFactor)));
                        preRootPos = rootPos;
                        preLFootPos = lFootPos;
                        preRFootPos = rFootPos;
                        preNeckPos = neckPos;

                        if (ignoreRotation) // If we ignore x-axis, simply set the forward to (0,0,1)
                            allPoints.Add(new TrajectoryPoint(rootPos, Vector3.forward));
                        else
                            allPoints.Add(new TrajectoryPoint(rootPos, startSpace.inverse.MultiplyVector(animator.GetBoneTransform(joints[0]).forward)));
                    }
                    else // Velocity calculations use j+1 - j instead of j - j-1, since there is no previous timestep, and the velocity in frame 0 should be similar to frame 1
                    {
                        preRootPos = rootPos;
                        preLFootPos = lFootPos;
                        preRFootPos = rFootPos;
                        preNeckPos = neckPos;
                        allClips[i].SampleAnimation(avatar, 1 / allClips[i].frameRate); // Sampling animation at frame 1 to get difference between frame 0 and 1
                        rootPos = startSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[0]).position.GetXZVector3());
                        lFootPos = charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[1]).position);
                        rFootPos = charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[2]).position);
                        neckPos = charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[3]).position);
                        allPoses.Add(new MMPose(preRootPos, preLFootPos, preRFootPos, preNeckPos,
                            CalculateVelocity(rootPos, preRootPos, velFactor),
                            CalculateVelocity(lFootPos, preLFootPos, velFactor),
                            CalculateVelocity(rFootPos, preRFootPos, velFactor),
                            CalculateVelocity(neckPos, preNeckPos, velFactor)));

                        if (ignoreRotation) // If we ignore x-axis, simply set the forward to (0,0,1)
                            allPoints.Add(new TrajectoryPoint(rootPos, Vector3.forward));
                        else
                            allPoints.Add(new TrajectoryPoint(rootPos, startSpace.inverse.MultiplyVector(animator.GetBoneTransform(joints[0]).forward)));
                    }
                }
            }

            csvHandler.WriteCSV(allPoses, allPoints, allClipNames, allClipFrameCounts, allFrames, allStates);
        }

        public List<FeatureVector> LoadData(int pointsPerTrajectory, int framesBetweenTrajectoryPoints)
        {
            if (csvHandler == null)
                csvHandler = new CSVHandler();
            return csvHandler.ReadCSV(pointsPerTrajectory, framesBetweenTrajectoryPoints);
        }

        public Vector3 CalculateVelocity(Vector3 currentPos, Vector3 previousPose, float velocityFactor)
        {
            return (currentPos - previousPose) * velocityFactor;
        }
    }
}