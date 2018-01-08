using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Tasks;

namespace UnityEditor.Build
{
    public enum Pipelines
    {
        PlayerScriptsOnly,
        AssetBundleCompatible,
        AutopackReleaseContent,
        //AutopackFastDeployContent,
    }

    public static class BuildPipeline
    {
        public static IList<IBuildTask> CreatePipeline(Pipelines pipeline)
        {
            switch (pipeline)
            {
                case Pipelines.AssetBundleCompatible:
                    return AssetBundleCompatible();
                case Pipelines.PlayerScriptsOnly:
                    return PlayerScriptsOnly();
                case Pipelines.AutopackReleaseContent:
                    return AutopackReleaseContent();
                default:
                    throw new NotImplementedException(string.Format("Pipeline '{0}' not yet implemented.", pipeline));
            }
        }

        public static IList<IBuildTask> AutopackReleaseContent()
        {
            var pipeline = new List<IBuildTask>();

            // Setup
            pipeline.Add(new ProjectInCleanState());
            pipeline.Add(new SwitchToBuildPlatform());
            pipeline.Add(new RebuildAtlasCache());

            // Player Scripts
            pipeline.Add(new BuildPlayerScripts());
            pipeline.Add(new SetBundleSettingsTypeDB());
            pipeline.Add(new PostScriptsCallback());

            // Dependency
            pipeline.Add(new CalculateSceneDependencyData());
            pipeline.Add(new CalculateAssetDependencyData());
            pipeline.Add(new StripUnusedSpriteSources());
            pipeline.Add(new PostDependencyCallback());

            // Packing
            // TODO: Implement autopacking build tasks
            pipeline.Add(new CalcualteObjectDependencyHash());
            //pipeline.Add(new GenerateBundlePacking());
            //pipeline.Add(new PackAssetBundleReferences());
            //pipeline.Add(new PackSceneBundleReferences());
            //pipeline.Add(new GenerateWriteCommands(new PrefabPackedIdentifiers()));
            pipeline.Add(new PostPackingCallback());

            // Writing
            // TODO: Replace with IBuildTasks for Archive & SerializedFile APIs
            pipeline.Add(new WriteAssetBundles());
            pipeline.Add(new WriteSceneBundles());
            pipeline.Add(new ArchiveAndCompressBundles());
            pipeline.Add(new PostWritingCallback());

            pipeline.Add(new LogContextToFile(@"D:\Projects\BuildHLAPI\Builds\BuildContext.json"));

            return pipeline;
        }

        static IList<IBuildTask> PlayerScriptsOnly()
        {
            var pipeline = new List<IBuildTask>();

            // Setup
            pipeline.Add(new ProjectInCleanState());
            pipeline.Add(new SwitchToBuildPlatform());

            // Player Scripts
            pipeline.Add(new BuildPlayerScripts());
            pipeline.Add(new SetBundleSettingsTypeDB());
            pipeline.Add(new PostScriptsCallback());

            return pipeline;
        }

        public static IList<IBuildTask> AssetBundleCompatible()
        {
            var pipeline = new List<IBuildTask>();

            // Setup
            pipeline.Add(new ProjectInCleanState());
            pipeline.Add(new ValidateBundleAssignments());
            pipeline.Add(new SwitchToBuildPlatform());
            pipeline.Add(new RebuildAtlasCache());

            // Player Scripts
            pipeline.Add(new BuildPlayerScripts());
            pipeline.Add(new SetBundleSettingsTypeDB());
            pipeline.Add(new PostScriptsCallback());

            // Dependency
            pipeline.Add(new CalculateSceneDependencyData());
            pipeline.Add(new CalculateAssetDependencyData());
            pipeline.Add(new StripUnusedSpriteSources());
            pipeline.Add(new PostDependencyCallback());

            // Packing
            pipeline.Add(new GenerateBundlePacking());
            pipeline.Add(new GenerateBundleCommands());
            pipeline.Add(new PostPackingCallback());

            // Writing
            // TODO: Rewrite the write tasks?
            pipeline.Add(new WriteAssetBundles());
            pipeline.Add(new WriteSceneBundles());
            pipeline.Add(new ArchiveAndCompressBundles());
            pipeline.Add(new PostWritingCallback());

            // Generate manifest files
            // TODO: IMPL manifest generation
            pipeline.Add(new LogContextToFile(@"D:\Projects\BuildHLAPI\Builds\BuildContext.json"));

            return pipeline;
        }
    }
}