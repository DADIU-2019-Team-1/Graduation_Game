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
            "ClipName" /*[0]*/, "Frame" /*[1]*/,

            // Pose data
            "RootPos.x" /*[2]*/, "RootPos.z" /*[3]*/,
            "LFootPos.x" /*[4]*/, "LFootPos.y" /*[5]*/, "LFootPos.z" /*[6]*/,
            "RFootPos.x" /*[7]*/, "RFootPos.y" /*[8]*/, "RFootPos.z" /*[9]*/,
            "NeckPos.x" /*[10]*/, "NeckPos.y" /*[11]*/, "NeckPos.z" /*[12]*/,

            "RootVel.x" /*[13]*/, "RootVel.z" /*[14]*/,
            "LFootVel.x" /*[15]*/, "LFootVel.y" /*[16]*/, "LFootVel.z" /*[17]*/,
            "RFootVel.x" /*[18]*/, "RFootVel.y" /*[19]*/, "RFootVel.z" /*[20]*/,
            "NeckVel.x" /*[21]*/, "NeckVel.y" /*[22]*/, "NeckVel.z" /*[23]*/,

            // TrajectoryPoint data
            "Forward.x" /*[24]*/, "Forward.z" /*[25]*/
        };

        private List<string> allClipNames;
        private List<int> allFrames;
        private List<MMPose> allPoses;
        private List<TrajectoryPoint> allPoints;

        public void WriteCSV(List<MMPose> poseData, List<TrajectoryPoint> pointData, List<string> clipNames, List<int> frames)
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
                    allPoses.Add(new MMPose(
                        // Positions
                        new Vector3(float.Parse(tempString[2], format), 0.0f, float.Parse(tempString[3], format)),
                        new Vector3(float.Parse(tempString[4], format), float.Parse(tempString[5], format),
                            float.Parse(tempString[6], format)),
                        new Vector3(float.Parse(tempString[7], format), float.Parse(tempString[8], format),
                            float.Parse(tempString[9], format)),
                        new Vector3(float.Parse(tempString[10], format), float.Parse(tempString[11], format),
                            float.Parse(tempString[12], format)),

                        // Velocities
                        new Vector3(float.Parse(tempString[13], format), 0.0f, float.Parse(tempString[14], format)),
                        new Vector3(float.Parse(tempString[15], format), float.Parse(tempString[16], format),
                            float.Parse(tempString[17], format)),
                        new Vector3(float.Parse(tempString[18], format), float.Parse(tempString[19], format),
                            float.Parse(tempString[20], format)),
                        new Vector3(float.Parse(tempString[21], format), float.Parse(tempString[22], format),
                            float.Parse(tempString[23], format))));

                    allPoints.Add(new TrajectoryPoint(
                        new Vector3(float.Parse(tempString[2], format), 0.0f, float.Parse(tempString[3], format)),
                        new Vector3(float.Parse(tempString[24], format), 0.0f, float.Parse(tempString[25], format))));
                }
                else
                    ignoreHeaders = false;
            }

            // Convert data to FeatureVector
            for (int i = 0; i < allClipNames.Count; i++)
            {
                TrajectoryPoint[] trajPoints = new TrajectoryPoint[trajPointsLength];
                for (int j = 0; j < trajPointsLength; j++)
                {
                    if (i + j * trajStepSize * 2.0f < allClipNames.Count) // Out of bounds handler
                    {
                        if (allFrames[i] <= allFrames[i + j * trajStepSize]
                        ) // clip 3 at frame 45 out of 70 with a trajStepSize of 10 goes 45, 55, 65, X, X
                            trajPoints[j] = new TrajectoryPoint(allPoints[i + j * trajStepSize].GetPoint(),
                                allPoints[i + j * trajStepSize].GetForward());
                        else
                            trajPoints[j] = new TrajectoryPoint(); // TODO: Extrapolate instead of resetting
                    }
                    else
                        trajPoints[j] = new TrajectoryPoint();
                }

                featuresFromCSV.Add(new FeatureVector(allPoses[i], new Trajectory(trajPoints), i, allClipNames[i],
                    allFrames[i]));
            }

            return featuresFromCSV;
        }
    }
}