using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TempleRunner.Editor
{
    public static class AndroidBuildScript
    {
        [MenuItem("Build/Build Android APK")]
        public static void BuildAndroidAPK()
        {
            string[] scenes = new string[] { "Assets/Scenes/MainGame.unity" };
            string outputPath = "Builds/TempleRunner3D.apk";

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "org.cleanroom.templerunner3d");
            PlayerSettings.productName = "Temple Runner 3D";
            PlayerSettings.companyName = "CleanRoom Studios";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log("[AndroidBuildScript] Starting Android APK build...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[AndroidBuildScript] Build SUCCESS: {summary.totalSize} bytes at {outputPath}");
            }
            else
            {
                Debug.LogError($"[AndroidBuildScript] Build FAILED with {summary.totalErrors} errors");
            }
        }
    }
}
