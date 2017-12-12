using System.Diagnostics;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEditor.Experimental.Build.Player;

namespace UnityEditor.Build.AssetBundle
{
    public static class BundleBuildPipeline
    {
        public const string kTempBundleBuildPath = "Temp/BundleBuildData";

        public const string kDefaultOutputPath = "AssetBundles";

        public static BuildCallbacks buildCallbacks = new BuildCallbacks();


        public static BuildSettings GenerateBundleBuildSettings(TypeDB typeDB)
        {
            var settings = new BuildSettings();
            settings.target = EditorUserBuildSettings.activeBuildTarget;
            settings.group = UnityEditor.BuildPipeline.GetBuildTargetGroup(settings.target);
            settings.typeDB = typeDB;
            return settings;
        }

        public static BuildSettings GenerateBundleBuildSettings(TypeDB typeDB, BuildTarget target)
        {
            var settings = new BuildSettings();
            settings.target = target;
            settings.group = UnityEditor.BuildPipeline.GetBuildTargetGroup(settings.target);
            settings.typeDB = typeDB;
            return settings;
        }

        public static BuildSettings GenerateBundleBuildSettings(TypeDB typeDB, BuildTarget target, BuildTargetGroup group)
        {
            var settings = new BuildSettings();
            settings.target = target;
            settings.group = group;
            settings.typeDB = typeDB;
            // TODO: Validate target & group
            return settings;
        }

        public static BuildPipelineCodes BuildAssetBundles(BuildInput input, BuildSettings settings, BuildCompression compression, string outputFolder, out BuildResultInfo result, object callbackUserData = null, bool useCache = true)
        {
            var buildTimer = new Stopwatch();
            buildTimer.Start();

            var exitCode = BuildPipelineCodes.Success;
            result = new BuildResultInfo();

            AssetDatabase.SaveAssets();

            using (var progressTracker = new BuildProgressTracker(10))
            {
                using (var buildCleanup = new BuildStateCleanup(true, kTempBundleBuildPath))
                {
                    var buildInput = new BuildLayout(input);
                    var buildParams = new BuildParams(settings, compression, outputFolder, kTempBundleBuildPath, useCache, progressTracker);

                    var buildContext = new BuildContext(buildInput, buildParams, buildCallbacks, buildCallbacks, buildCallbacks);

                    var buildRunner = BuildPipeline.Create();
                    exitCode = buildRunner.Run(buildContext);
                }
            }

            buildTimer.Stop();
            if (exitCode >= BuildPipelineCodes.Success)
                BuildLogger.Log("Build Asset Bundles successful in: {0:c}", buildTimer.Elapsed);
            else if (exitCode == BuildPipelineCodes.Canceled)
                BuildLogger.LogWarning("Build Asset Bundles canceled in: {0:c}", buildTimer.Elapsed);
            else
                BuildLogger.LogError("Build Asset Bundles failed in: {0:c}. Error: {1}.", buildTimer.Elapsed, exitCode);

            return exitCode;
        }
    }
}