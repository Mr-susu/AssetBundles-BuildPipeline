using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Tasks;

namespace UnityEditor.Build
{
    public static class BuildPipeline
    {
        internal static void AddSetupTasks(IList<IBuildTask> pipeline, bool rebuildAtlas = true)
        {
            pipeline.Add(new ProjectInCleanState());
            pipeline.Add(new SwitchToBuildPlatform());
            if (rebuildAtlas)
                pipeline.Add(new RebuildAtlasCache());
        }

        internal static void AddPlayerScriptsTasks(IList<IBuildTask> pipeline)
        {
            pipeline.Add(new BuildPlayerScripts());
            pipeline.Add(new SetBundleSettingsTypeDB());
            pipeline.Add(new PostScriptsCallback());
        }

        internal static void AddDependencyTasks(IList<IBuildTask> pipeline)
        {
            pipeline.Add(new CalculateSceneDependencyData());
            pipeline.Add(new CalculateAssetDependencyData());
            pipeline.Add(new GenerateBundleDependencyLookups());
            pipeline.Add(new GenerateAssetToBundleDependency());
            pipeline.Add(new GenerateSceneToBundleDependency());
            pipeline.Add(new PostDependencyCallback());
        }

        internal static void AddPackingTasks(IList<IBuildTask> pipeline, IDeterministicIdentifiers packingMethod)
        {
            pipeline.Add(new ValidateBundleAssignments());
            pipeline.Add(new StripUnusedSpriteSources());
            pipeline.Add(new GenerateWriteCommands(packingMethod)); // TODO: maybe split up the generator per command type (Scene, bundle, raw, etc)
            pipeline.Add(new PostPackingCallback());
        }

        internal static void AddWritingTasks(IList<IBuildTask> pipeline)
        {
            pipeline.Add(new WriteAssetBundles());
            pipeline.Add(new WriteSceneBundles());
            pipeline.Add(new ArchiveAndCompressBundles());
            pipeline.Add(new PostWritingCallback());
        }

        public static IList<IBuildTask> CreateFull()
        {
            var pipeline = new List<IBuildTask>();
            // Setup
            AddSetupTasks(pipeline);
            // Player Scripts
            AddPlayerScriptsTasks(pipeline);
            // Dependency
            AddDependencyTasks(pipeline);
            // Packing
            AddPackingTasks(pipeline, new Unity5PackedIdentifiers());
            // Writing
            AddWritingTasks(pipeline);
            return pipeline;
        }

        public static IList<IBuildTask> CreateBundle()
        {
            var pipeline = new List<IBuildTask>();
            // Setup
            AddSetupTasks(pipeline);
            // Dependency
            AddDependencyTasks(pipeline);
            // Packing
            AddPackingTasks(pipeline, new Unity5PackedIdentifiers());
            // Writing
            AddWritingTasks(pipeline);
            return pipeline;
        }

        public static IList<IBuildTask> CreatePlayerScripts()
        {
            var pipeline = new List<IBuildTask>();
            // Setup
            AddSetupTasks(pipeline, false);
            // Player Scripts
            AddPlayerScriptsTasks(pipeline);
            return pipeline;
        }

        public static IList<IBuildTask> CreateLegacy()
        {
            var pipeline = new List<IBuildTask>();
            // Setup
            AddSetupTasks(pipeline);
            // Player Scripts
            AddPlayerScriptsTasks(pipeline);
            // Dependency
            AddDependencyTasks(pipeline);
            // Packing
            AddPackingTasks(pipeline, new Unity5PackedIdentifiers());
            // Writing
            AddWritingTasks(pipeline);
            // Generate manifest files
            // TODO: IMPL manifest generation
            return pipeline;
        }
    }
}