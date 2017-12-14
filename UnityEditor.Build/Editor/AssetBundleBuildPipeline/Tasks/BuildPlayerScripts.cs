using System;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.Player;

namespace UnityEditor.Build.Tasks
{
    public class BuildPlayerScripts : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { typeof(IBuildParams), typeof(IResultInfo) };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IResultInfo>());
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IResultInfo output)
        {
            if (buildParams.ProgressTracker != null) // can't use null propagation
            {
                buildParams.ProgressTracker.StartStep("Compiling Player Scripts", 1);
                if (!buildParams.ProgressTracker.UpdateProgress(""))
                {
                    buildParams.ProgressTracker.EndProgress();
                    return BuildPipelineCodes.Canceled;
                }
            }

            // TODO: Replace with call to GetTempOrCachePath
            // TODO: Create tasks to copy scripts to correct output folder?
            output.ScriptResults = PlayerBuildInterface.CompilePlayerScripts(buildParams.ScriptSettings, buildParams.TempOutputFolder);
            if (output.ScriptResults.assemblies.IsNullOrEmpty() && output.ScriptResults.typeDB == null)
                return BuildPipelineCodes.Error;

            if (buildParams.ProgressTracker != null) // can't use null propagation
            {
                if (!buildParams.ProgressTracker.EndProgress())
                    return BuildPipelineCodes.Canceled;
            }
            return BuildPipelineCodes.Success;
        }
    }
}
