#if UNITY_EDITOR

using System.Net;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Immutable.Passport.Editor
{
    internal class SdkPostprocess : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.result is BuildResult.Failed or BuildResult.Cancelled)
                return;

            BuildTarget buildTarget = report.summary.platform;

            string buildFullOutputPath = report.summary.outputPath;
            string buildAppName = Path.GetFileNameWithoutExtension(buildFullOutputPath);
            string buildOutputPath = Path.GetDirectoryName(buildFullOutputPath);

            Debug.Log("Copying passport browser files...");

            // Get the build's data folder
            string buildDataPath = Path.GetFullPath($"{buildOutputPath}/{buildAppName}_Data/");
            if (buildTarget == BuildTarget.StandaloneOSX)
            {
                buildDataPath =
                    Path.GetFullPath($"{buildOutputPath}/{buildAppName}.app/Contents/Resources/Data/");
            } else if (buildTarget == BuildTarget.Android)
            {
                buildDataPath = Path.GetFullPath($"{buildOutputPath}/{buildAppName}/unityLibrary/src/main/assets/");
            }

            // Check that the data folder exists
            if (!Directory.Exists(buildDataPath))
            {
                Debug.LogError(
                    "Failed to get the build's data folder. Make sure your build is the same name as your product name (In your project settings).");
                return;
            }

            Debug.Log("Copying Passport directory...");
            CopyDirectory("./Assets/ImmutableSDK/Runtime/Passport", $"{buildDataPath}/ImmutableSDK/Runtime/Passport");

            Debug.Log("Copying UWB directory...");
            CopyDirectory("./Assets/ImmutableSDK/Runtime/UWB", $"{buildDataPath}/ImmutableSDK/Runtime/UWB");

            Debug.Log($"Sucessfully copied Immutable SDK files");
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            var dir = new DirectoryInfo(sourceDir);
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (FileInfo file in dir.GetFiles())
            {
                if (!file.Name.EndsWith(".meta"))
                {
                    string targetFilePath = Path.Combine(destinationDir, file.Name);
                    file.CopyTo(targetFilePath, true);
                }
            }

            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }
    }
}

#endif