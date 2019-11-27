// Code Owner: Jannik Neerdal
using System.Collections.Generic;
using System.Globalization;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Team1_GraduationGame.MotionMatching
{
    public class CSVHandler
    {
        private string path = "Assets/Resources/MotionMatching";
        private string fileName = "AnimData.csv";

        private static string[] csvLabels =
        {
            // General info
            "ClipName" /*[0]*/, "Frame" /*[1]*/, "State" /*[2]*/,

            // Pose data
            "RootPos.x" /*[3]*/, "RootPos.z" /*[4]*/,
            "LFootPos.x" /*[5]*/, "LFootPos.y" /*[6]*/, "LFootPos.z" /*[7]*/,
            "RFootPos.x" /*[8]*/, "RFootPos.y" /*[9]*/, "RFootPos.z" /*[10]*/,
            "NeckPos.x" /*[11]*/, "NeckPos.y" /*[12]*/, "NeckPos.z" /*[13]*/,

            "RootVel.x" /*[14]*/, "RootVel.z" /*[15]*/,
            "LFootVel.x" /*[16]*/, "LFootVel.y" /*[17]*/, "LFootVel.z" /*[18]*/,
            "RFootVel.x" /*[19]*/, "RFootVel.y" /*[20]*/, "RFootVel.z" /*[21]*/,
            "NeckVel.x" /*[22]*/, "NeckVel.y" /*[23]*/, "NeckVel.z" /*[24]*/,

            // TrajectoryPoint data
            "Forward.x" /*[25]*/, "Forward.z" /*[26]*/
        };

        private List<string> allClipNames;
        private List<int> allFrames, allStates;
        private List<MMPose> allPoses;
        private List<TrajectoryPoint> allPoints;

        public void WriteCSV(List<MMPose> poseData, List<TrajectoryPoint> pointData, List<string> clipNames, List<int> frames, List<int> states)
        {
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder(path))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                AssetDatabase.CreateFolder("Assets/Resources", "MotionMatching");
            }
#endif
            using (var file = File.CreateText(path + "/" + fileName))
            {
                file.WriteLine(string.Join(",", csvLabels));

                // System language generalization
                string spec = "G";
                CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");

                for (int i = 0; i < poseData.Count; i++)
                {
                    string[] tempLine =
                    {
                        // General info
                        clipNames[i], frames[i].ToString(spec, ci),
                        states[i].ToString(spec, ci),

                        // Pose data
                        poseData[i].GetRootPos().x.ToString(spec, ci),
                        poseData[i].GetRootPos().z.ToString(spec, ci),
                        poseData[i].GetLeftFootPos().x.ToString(spec, ci),
                        poseData[i].GetLeftFootPos().y.ToString(spec, ci),
                        poseData[i].GetLeftFootPos().z.ToString(spec, ci),
                        poseData[i].GetRightFootPos().x.ToString(spec, ci),
                        poseData[i].GetRightFootPos().y.ToString(spec, ci),
                        poseData[i].GetRightFootPos().z.ToString(spec, ci),
                        poseData[i].GetNeckPos().x.ToString(spec, ci),
                        poseData[i].GetNeckPos().y.ToString(spec, ci),
                        poseData[i].GetNeckPos().z.ToString(spec, ci),

                        poseData[i].GetRootVelocity().x.ToString(spec, ci),
                        poseData[i].GetRootVelocity().z.ToString(spec, ci),
                        poseData[i].GetLeftFootVelocity().x.ToString(spec, ci),
                        poseData[i].GetLeftFootVelocity().y.ToString(spec, ci),
                        poseData[i].GetLeftFootVelocity().z.ToString(spec, ci),
                        poseData[i].GetRightFootVelocity().x.ToString(spec, ci),
                        poseData[i].GetRightFootVelocity().y.ToString(spec, ci),
                        poseData[i].GetRightFootVelocity().z.ToString(spec, ci),
                        poseData[i].GetNeckVelocity().x.ToString(spec, ci),
                        poseData[i].GetNeckVelocity().y.ToString(spec, ci),
                        poseData[i].GetNeckVelocity().z.ToString(spec, ci),

                        // TrajectoryPoint data
                        pointData[i].GetForward().x.ToString(spec, ci),
                        pointData[i].GetForward().z.ToString(spec, ci)
                    };

                    file.WriteLine(string.Join(",", tempLine));
                }
            }
        }

        public List<FeatureVector> ReadCSV(int trajPointsLength, int trajStepSize)
        {
            StreamReader reader =
                new StreamReader(new MemoryStream((Resources.Load("MotionMatching/AnimData") as TextAsset).bytes));

            bool ignoreHeaders = true;

            allClipNames = new List<string>();
            allFrames = new List<int>();
            allStates = new List<int>();
            allPoses = new List<MMPose>();
            allPoints = new List<TrajectoryPoint>();
            List<FeatureVector> featuresFromCSV = new List<FeatureVector>();

            while (true) // True until break is called within the loop
            {
                string dataString = reader.ReadLine(); // Reads a line (or row) in the CSV file
                if (dataString == null) // No more data to be read, so break from the while loop
                    break;

                string[] tempString = dataString.Split(','); // line is split into each column
                NumberFormatInfo format = CultureInfo.InvariantCulture.NumberFormat;

                if (!ignoreHeaders) // Iterates for each row in the CSV aside from the first (header) row
                {
                    allClipNames.Add(tempString[0]);
                    allFrames.Add(int.Parse(tempString[1], format));
                    allStates.Add(int.Parse(tempString[2], format));
                    allPoses.Add(new MMPose(
                        // Positions
                        new Vector3(float.Parse(tempString[3], format), 0.0f, float.Parse(tempString[4], format)),
                        new Vector3(float.Parse(tempString[5], format), float.Parse(tempString[6], format), float.Parse(tempString[7], format)),
                        new Vector3(float.Parse(tempString[8], format), float.Parse(tempString[9], format), float.Parse(tempString[10], format)),
                        new Vector3(float.Parse(tempString[11], format), float.Parse(tempString[12], format), float.Parse(tempString[13], format)),

                        // Velocities
                        new Vector3(float.Parse(tempString[14], format), 0.0f, float.Parse(tempString[15], format)),
                        new Vector3(float.Parse(tempString[16], format), float.Parse(tempString[17], format),
                            float.Parse(tempString[18], format)),
                        new Vector3(float.Parse(tempString[19], format), float.Parse(tempString[20], format),
                            float.Parse(tempString[21], format)),
                        new Vector3(float.Parse(tempString[22], format), float.Parse(tempString[23], format),
                            float.Parse(tempString[24], format))));

                    allPoints.Add(new TrajectoryPoint(
                        new Vector3(float.Parse(tempString[3], format), 0.0f, float.Parse(tempString[4], format)),
                        new Vector3(float.Parse(tempString[25], format), 0.0f, float.Parse(tempString[26], format))));
                }
                else
                    ignoreHeaders = false;
            }

            // Convert data to FeatureVector
            Matrix4x4 animSpace = new Matrix4x4();
            TrajectoryPoint[] trajPoints = new TrajectoryPoint[trajPointsLength];
            string nameDiff = allClipNames[0];            
            for (int i = 0; i < allClipNames.Count; i++)
            {
                trajPoints = new TrajectoryPoint[trajPointsLength];
                animSpace.SetTRS(allPoints[i].GetPoint(), Quaternion.identity, Vector3.one);
                for (int j = 0; j < trajPointsLength; j++)
                {
                    if (i + j * trajStepSize < allClipNames.Count) // Out of bounds handler
                    {
                        if (allClipNames[i] == allClipNames[i + j * trajStepSize]) // When creating the trajectory, check if all the points pertain to the same animation - if not, set the remaining points to 0
                        {
                            trajPoints[j] = new TrajectoryPoint(animSpace.inverse.MultiplyPoint3x4(allPoints[i + j * trajStepSize].GetPoint()),
                                animSpace.MultiplyVector(allPoints[i + j * trajStepSize].GetForward()));
                        }
                        else
                            trajPoints[j] = new TrajectoryPoint(); // TODO: Extrapolate instead of resetting
                    }
                    else
                        trajPoints[j] = new TrajectoryPoint();
                }

                featuresFromCSV.Add(new FeatureVector(allPoses[i], new Trajectory(trajPoints), i, allClipNames[i],
                    allFrames[i], allStates[i]));

                // TODO: Add this as a column in the CSV
                //if (nameDiff != allClipNames[i]) // Setting Frame count based on clip names
                //{
                //    featuresFromCSV[i - 1].SetFrameCount(featuresFromCSV[i - 1].GetFrame() + 1);
                //    Debug.Log("Assigned feature vectors from clip " + nameDiff + " to have a frame count of " + featuresFromCSV);
                //    nameDiff = allClipNames[i];
                //}
            }

            return featuresFromCSV;
        }
    }
}