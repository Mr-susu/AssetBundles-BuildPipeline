﻿using System.Collections.Generic;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public interface IBuildTask
    {
        int Version { get; }

        IBuildParams BuildParams { get; set; }

        BuildPipelineCodes Run(object context);
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
                catch
                {
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
            runner.AddTask(new ProjectInCleanState());
            runner.AddTask(new SwitchToBuildPlatform());
            runner.AddTask(new RebuildAtlasCache());
            runner.AddTask(new CalculateSceneDependencyData());
            runner.AddTask(new CalculateAssetDependencyData());
            runner.AddTask(new CalculateBundleLookups());
            runner.AddTask(new PostDependencyCallback());
            runner.AddTask(new StripUnusedSpriteSources());
            runner.AddTask(new GenerateWriteCommands(new Unity5PackedIdentifiers()));
            return runner;
        }
    }
}
