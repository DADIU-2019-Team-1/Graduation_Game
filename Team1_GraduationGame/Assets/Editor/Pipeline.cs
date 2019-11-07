using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
namespace UnityEditor
{
    public class Pipeline
    {
	    private static string workingDirectory;

        //[MenuItem("Pipeline/Build: Latest fetch from SCM")]//
        public static void BuildAndroidAutobuild()
	    {
			BuildAndroidBase(@"C:\Users\Dadiu student\.jenkins\workspace\Graduation_Game\Autobuild\Team1_GraduationGame");
	    }
	    public static void BuildAndroidMaster()
	    {
		    BuildAndroidBase(@"C:\Users\Dadiu student\.jenkins\workspace\Graduation_Game\Manual Master\Team1_GraduationGame");
	    }
	    public static void BuildAndroidDevelopment()
	    {
		    BuildAndroidBase(@"C:\Users\Dadiu student\.jenkins\workspace\Graduation_Game\Manual Development\Team1_GraduationGame");
	    }
	    public static void BuildAndroidRelease()
	    {
		    BuildAndroidBase(@"C:\Users\Dadiu student\.jenkins\workspace\Graduation_Game\Manual Release\Team1_GraduationGame");
	    }
        //[MenuItem("Pipeline/Build: Android from Unity")]
        public static void BuildAndroidPC()
        {
            BuildAndroidBase(@"C:\Users\Dadiu student\Documents\GitHub\Graduation_Game\Team1_GraduationGame");
        }
        //[MenuItem("Pipeline/DebugBuildScript")]
        public static void DebugScript()
        {
            workingDirectory = @"C:\Users\Dadiu student\.jenkins\workspace\Graduation_Game\Autobuild\Team1_GraduationGame";
            UnityEngine.Debug.Log(repoCommitMessage);
        }

        private static void BuildAndroidBase(string workingDir)
        {
	        workingDirectory = workingDir;
            Directory.CreateDirectory(pathname);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                locationPathName = Path.Combine(pathname, filename),
                scenes = EditorBuildSettings.scenes.Where(n =>
               n.enabled).Select(n => n.path).ToArray(),
                target = BuildTarget.Android
            });

            UnityEngine.Debug.Log(report);
        }

        /*
        * This is a static property which will return a string, representing a
        * build folder on the desktop. This does not create the folder when it
        * doesn't exists, it simply returns a suggested path. It is put on the
        * desktop, so it's easier to find but you can change the string to any
        * path really. Path combine is used, for better cross platform support
        */
        public static string pathname
        {
            get
            {
                return @"C:\Users\Dadiu student\Google Drive\DADIU 2019\builds\" + repoBranchName;
                //return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"Builds\" + repoBranchName);
            }
        }

        /*
        * This returns the filename that the build should spit it. For a start
        * this just returns a current date, in a simple lexicographical format
        * with the apk extension appended. Later on, you can customize this to
        * include more information, such as last person to commit, what branch
        * were used, version of the game, or what git-hash the game were using
        */
        public static string filename
        {
            get
            {
                return (DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "_" + repoBranchName + "_" + repoCommitMessage + ".apk");
            }
        }


        public static string repoBranchName
        {
            get
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("git.exe");

                startInfo.UseShellExecute = false;
                startInfo.WorkingDirectory = workingDirectory;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.Arguments = "rev-parse --abbrev-ref HEAD";

                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                string branchname = process.StandardOutput.ReadLine();
                return branchname;
            }
        }

        public static string repoCommitMessage
        {
            get
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("git.exe");

                startInfo.UseShellExecute = false;
                startInfo.WorkingDirectory = workingDirectory;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.Arguments = "show -s --format=%B --abbrev-commit";

                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                string commitMessage = process.StandardOutput.ReadLine();
                return commitMessage;
            }
        }
        // Unused, since the method does not update the build number in the git
        public static string UpdateBuildNumberIdentifier()
        {
            string text = "";
            string number = "";
            string buildNumFilePath = Application.dataPath + "/Resources/buildNumbers.txt";
            FileStream file = File.Open(buildNumFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            file.Close();

            TextAsset buildNRFile = Resources.Load("buildNumbers") as TextAsset;
            if (buildNRFile == null)
            {
                buildNRFile = new TextAsset("The current build number of the project is\n0");
            }
            string allLines = buildNRFile.text;
            string[] everyLine = new string[2];
            if (allLines.Any())
            {
                everyLine = allLines.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }
            if (everyLine.Length > 0)
            {
                UnityEngine.Debug.Log("The build number file contains: \n" + everyLine[0] + " " + everyLine[1]);
            }
            int curBuildNum = int.Parse(everyLine[1]) + 1;

            everyLine[0] = "The current build number of the project is";
            everyLine[1] = curBuildNum.ToString();

            UnityEngine.Debug.Log("Cur build nr = " + curBuildNum);

            File.WriteAllLines(buildNumFilePath, everyLine);

            UnityEngine.Debug.Log("I have updated the build number");
            return everyLine[1];
        }
    }
}