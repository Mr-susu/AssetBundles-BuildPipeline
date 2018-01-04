﻿using UnityEditor.Build.AssetBundle;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Player;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEditor.Experimental.Build.Player;
using UnityEngine;

namespace UnityEditor.Build
{
    public static class LegacyBuildPipeline
    {
        public const string kTempLegacyBuildPath = "Temp/LegacyBuildData";

        public static AssetBundleManifest BuildAssetBundles(string outputPath, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform)
        {
            var buildInput = BundleBuildInterface.GenerateBuildInput();
            return BuildAssetBundles_Internal(outputPath, new BuildLayout(buildInput), assetBundleOptions, targetPlatform);
        }

        public static AssetBundleManifest BuildAssetBundles(string outputPath, AssetBundleBuild[] builds, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform)
        {
            return BuildAssetBundles_Internal(outputPath, new BuildLayout(builds), assetBundleOptions, targetPlatform);
        }

        internal static AssetBundleManifest BuildAssetBundles_Internal(string outputPath, IBuildLayout buildInput, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform)
        {
            BuildCompression compression = BuildCompression.DefaultLZMA;
            if ((assetBundleOptions & BuildAssetBundleOptions.ChunkBasedCompression) != 0)
                compression = BuildCompression.DefaultLZ4;
            else if ((assetBundleOptions & BuildAssetBundleOptions.UncompressedAssetBundle) != 0)
                compression = BuildCompression.DefaultUncompressed;

            bool useCache = (assetBundleOptions & BuildAssetBundleOptions.ForceRebuildAssetBundle) == 0;

            ScriptCompilationSettings scriptSettings = PlayerBuildPipeline.GeneratePlayerBuildSettings(targetPlatform);
            BuildSettings bundleSettings = BundleBuildPipeline.GenerateBundleBuildSettings(null, targetPlatform); // Legacy & Full pipelines set typedb during run

            BuildPipelineCodes exitCode;
            using (var progressTracker = new ProgressTracker())
            {
                using (var buildCleanup = new BuildStateCleanup(true, kTempLegacyBuildPath))
                {
                    var buildParams = new BuildParams(scriptSettings, bundleSettings, compression, outputPath, kTempLegacyBuildPath, useCache);

                    var buildContext = new BuildContext(buildInput, buildParams, progressTracker);
                    buildContext.SetContextObject(new BuildDependencyInfo());
                    buildContext.SetContextObject(new BuildWriteInfo());
                    buildContext.SetContextObject(new BuildResultInfo());
                    buildContext.SetContextObject(PlayerBuildPipeline.BuildCallbacks);
                    buildContext.SetContextObject(BundleBuildPipeline.BuildCallbacks);

                    var pipeline = BuildPipeline.CreateLegacy();
                    exitCode = BuildRunner.Validate(pipeline, buildContext);
                    if (exitCode >= BuildPipelineCodes.Success)
                        exitCode = BuildRunner.Run(pipeline, buildContext);
                }
            }

            // TODO: Return Unity 5 Manifest
            return null;
        }
    }
}