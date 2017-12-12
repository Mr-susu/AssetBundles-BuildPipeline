using System;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public static class DefaultBuildPipeline
    {
        public static DefaultBuildRunner Create()
        {
            var runner = new DefaultBuildRunner();
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
            var packer = new Unity5PackedIdentifiers();
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