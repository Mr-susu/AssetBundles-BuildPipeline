using System;
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

        public static DefaultBuildCallbacks BuildCallbacks = new DefaultBuildCallbacks();


        public static BuildSettings GenerateBundleBuildSettings(TypeDB typeDB)
        {
            var settings = new BuildSettings();
            settings.target = EditorUserBuildSettings.activeBuildTarget;
            settings.group = BuildPipeline.GetBuildTargetGroup(settings.target);
            settings.typeDB = typeDB;
            return settings;
        }

        public static BuildSettings GenerateBundleBuildSettings(TypeDB typeDB, BuildTarget target)
        {
            var settings = new BuildSettings();
            settings.target = target;
            settings.group = BuildPipeline.GetBuildTargetGroup(settings.target);
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

        public static BuildPipelineCodes BuildAssetBundles(BuildInput input, BuildSettings settings, BuildCompression compression, string outputFolder, out DefaultBuildResultInfo result, object callbackUserData = null, bool useCache = true)
        {
            var buildTimer = new Stopwatch();
            buildTimer.Start();

            var exitCode = BuildPipelineCodes.Success;
            result = new DefaultBuildResultInfo();

            AssetDatabase.SaveAssets();

            using (var progressTracker = new BuildProgressTracker(10))
            {
                using (var buildCleanup = new BuildStateCleanup(true, kTempBundleBuildPath))
                {
                    var buildInput = new DefaultBundleInput(input);
                    var buildParams = new DefaultBuildParams(settings, compression, outputFolder, kTempBundleBuildPath, useCache, progressTracker);

                    var buildContext = new DefaultBuildContext(buildInput, buildParams);
                    buildContext.SetContextObject<IDependencyCallback>(BuildCallbacks);
                    buildContext.SetContextObject<IPackingCallback>(BuildCallbacks);
                    buildContext.SetContextObject<IWritingCallback>(BuildCallbacks);

                    var buildRunner = DefaultBuildPipeline.Create();
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