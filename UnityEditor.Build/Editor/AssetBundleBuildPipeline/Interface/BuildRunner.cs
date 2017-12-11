using System.Collections.Generic;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public interface IBuildTask
    {
        int Version { get; }

        IBuildParams BuildParams { get; set; }

        BuildPipelineCodes Run(object context);
    }

    public interface IBuildTask<I1>
    {
        int Version { get; }

        BuildPipelineCodes Run(I1 input);
    }

    public interface IBuildTask<I1, O1>
    {
        int Version { get; }

        BuildPipelineCodes Run(I1 input, O1 output);
    }

    public interface IBuildTask<I1, I2, O1>
    {
        int Version { get; }

        BuildPipelineCodes Run(I1 input1, I2 input2, O1 output);
    }

    public interface IBuildTask<I1, I2, I3, O1>
    {
        int Version { get; }

        BuildPipelineCodes Run(I1 input1, I2 input2, I3 input3, O1 output);
    }

    public class BuildRunner
    {
        private List<IBuildTask> m_Tasks = new List<IBuildTask>();

        private IBuildParams BuildParams { get; set; }

        public BuildRunner(IBuildParams buildParams)
        {
            BuildParams = buildParams;
        }

        public void AddTask(IBuildTask task)
        {
            task.BuildParams = BuildParams;
            m_Tasks.Add(task);
        }

        public BuildPipelineCodes Run(object context)
        {
            foreach (var task in m_Tasks)
            {
                try
                {
                    var result = task.Run(context);
                    if (result < 0)
                        return result;
                }
                catch (System.Exception e)
                {
                    BuildLogger.LogException(e);
                    return BuildPipelineCodes.Exception;
                }
            }
            return BuildPipelineCodes.Success;
        }
    }

    public static class DefaultBuildPipeline
    {
        public static BuildRunner Create(IBuildParams input)
        {
            var runner = new BuildRunner(input);
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
            runner.AddTask(new GenerateWriteCommands(packer)); // TODO: maybe split up the generator per command type (Scene, bundle, raw, etc)
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
