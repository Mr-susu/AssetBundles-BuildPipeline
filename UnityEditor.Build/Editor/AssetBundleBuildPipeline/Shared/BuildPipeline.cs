using System;
using UnityEditor.Build.Tasks;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public static class BuildPipeline
    {
        public static BuildRunner Create()
        {
            var runner = new BuildRunner();
            // Setup
            runner.AddTask(new ProjectInCleanState());
            runner.AddTask(new SwitchToBuildPlatform());
            runner.AddTask(new RebuildAtlasCache());
            // Dependency
            runner.AddTask(new CalculateSceneDependencyData());
            runner.AddTask(new CalculateAssetDependencyData());
            runner.AddTask(new CalculateBundleLookups());
            runner.AddTask(new PostDependencyCallback());
            // Packing
            runner.AddTask(new ValidateBundleAssignments());
            runner.AddTask(new StripUnusedSpriteSources());
            runner.AddTask(new GenerateWriteCommands()); // TODO: maybe split up the generator per command type (Scene, bundle, raw, etc)
            runner.AddTask(new PostPackingCallback());
            // Writing
            runner.AddTask(new WriteAssetBundles());
            runner.AddTask(new WriteSceneBundles());
            runner.AddTask(new ArchiveAndCompressBundles());
            runner.AddTask(new PostWritingCallback());
            return runner;
        }
    }
}