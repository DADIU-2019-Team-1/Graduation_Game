using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Team1_GraduationGame.MotionMatching
{
    public class PreProcessing
    {
        // --- References
        private CSVHandler csvHandler;

        // --- Collections
        private List<string> allClipNames;
        private List<int> allFrames, allStates;
        private List<MMPose> allPoses;
        private List<TrajectoryPoint> allPoints;
        private List<Vector3> allRootVels, allLFootVels, allRFootVels;

        // --- Variables
        private const float velFactor = 10.0f;

        public void Preprocess(AnimationClip[] allClips, HumanBodyBones[] joints, GameObject avatar, Animator animator,
            float frameSampleRate, string[] states)
        {
            csvHandler = new CSVHandler();

            allClipNames = new List<string>();
            allFrames = new List<int>();
            allStates = new List<int>();
            allPoses = new List<MMPose>();
            allPoints = new List<TrajectoryPoint>();

            Matrix4x4 startSpace = new Matrix4x4();
            Matrix4x4 charSpace = new Matrix4x4();
            for (int i = 0; i < allClips.Length; i++)
            {
                allClips[i].SampleAnimation(avatar, 0); // First frame of currently sampled animation
                Vector3 startPosForClip = new Vector3(animator.GetBoneTransform(joints[0]).position.x, 0.0f,
                    animator.GetBoneTransform(joints[0]).position.z);
                Quaternion startRotForClip = animator.GetBoneTransform(joints[0]).rotation;
                startSpace.SetTRS(startPosForClip, startRotForClip, Vector3.one);

                Vector3 preRootPos = Vector3.zero,
                    preLFootPos = Vector3.zero,
                    preRFootPos = Vector3.zero,
                    preNeckPos = Vector3.zero;
                string lowercaseName = allClips[i].name.ToLower();
                int clipState = -1;
                for (int j = 0; j < states.Length; j++)
                {
                    Debug.Log("Checking state for " + lowercaseName + " | Is it " + states[j] + "?");
                    if (lowercaseName.Contains(states[j]) && !lowercaseName.Contains("from" + states[j]))
                    {
                        Debug.Log("Yes! Set clip state to " + j + " which is " + states[j]);
                        clipState = j;
                        break;
                    }
                }
                if (clipState == -1)
                {
                    Debug.Log(i + " skipped");
                    continue;
                }

                for (int j = 0; j < (int) (allClips[i].length * frameSampleRate); j++)
                {
                    allClips[i].SampleAnimation(avatar, j / frameSampleRate);
                    allClipNames.Add(allClips[i].name);
                    allFrames.Add(j);
                    allStates.Add(clipState);
                    Vector3 rootPos =
                        startSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[0]).position);
                    charSpace.SetTRS(
                        new Vector3(animator.GetBoneTransform(joints[0]).position.x, 0.0f,
                            animator.GetBoneTransform(joints[0]).position.z),
                        animator.GetBoneTransform(joints[0]).rotation, Vector3.one);
                    Vector3 lFootPos =
                        charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[1]).position);
                    Vector3 rFootPos =
                        charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[2]).position);
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

                        allPoints.Add(new TrajectoryPoint(rootPos,
                            rootPos + startSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[0])
                                .forward)));
                    }
                    else // Velocity calculations use j+1 - j instead of j - j-1, since there is no previous timestep, and the velocity in frame 0 should be similar to frame 1
                    {
                        preRootPos = rootPos;
                        preLFootPos = lFootPos;
                        preRFootPos = rFootPos;
                        preNeckPos = neckPos;
                        allClips[i].SampleAnimation(avatar,
                            1 / allClips[i]
                                .frameRate); // Sampling animation at frame 1 to get difference between frame 0 and 1
                        rootPos = startSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[0]).position);
                        lFootPos = charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[1]).position);
                        rFootPos = charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[2]).position);
                        neckPos = charSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[3]).position);
                        allPoses.Add(new MMPose(preRootPos, preLFootPos, preRFootPos, preNeckPos,
                            CalculateVelocity(rootPos, preRootPos, velFactor),
                            CalculateVelocity(lFootPos, preLFootPos, velFactor),
                            CalculateVelocity(rFootPos, preRFootPos, velFactor),
                            CalculateVelocity(neckPos, preNeckPos, velFactor)));

                        allPoints.Add(new TrajectoryPoint(rootPos,
                            preRootPos +
                            startSpace.inverse.MultiplyPoint3x4(animator.GetBoneTransform(joints[0]).forward)));
                    }
                }
            }

            csvHandler.WriteCSV(allPoses, allPoints, allClipNames, allFrames, allStates);
        }

        public List<FeatureVector> LoadData(int pointsPerTrajectory, int framesBetweenTrajectoryPoints)
        {
            if (csvHandler == null)
                csvHandler = new CSVHandler();
            return csvHandler.ReadCSV(pointsPerTrajectory, framesBetweenTrajectoryPoints);
            ;
        }

        public Vector3 CalculateVelocity(Vector3 currentPos, Vector3 previousPose, float velocityFactor)
        {
            return (currentPos - previousPose) * velocityFactor;
        }

        public Vector3 CalculateVelocity(Vector3 currentPos, Vector3 previousPose, Matrix4x4 newSpace,
            float velocityFactor)
        {
            return (newSpace.MultiplyPoint3x4(currentPos) - newSpace.MultiplyPoint3x4(previousPose)) * velocityFactor;
        }

    }
}