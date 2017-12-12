using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.Player;

namespace UnityEditor.Build.Tasks
{
    public class BuildPlayerScripts : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            throw new System.NotImplementedException();
        }

        public static BuildPipelineCodes Run(IProgressTracker progressTracker, ScriptCompilationSettings settings, string outputFolder, out ScriptCompilationResult output)
        {
            if (progressTracker != null) // can't use null propagation
            {
                progressTracker.StartStep("Compiling Player Scripts", 1);
                if (!progressTracker.UpdateProgress(""))
                {
                    output = new ScriptCompilationResult();
                    progressTracker.EndProgress();
                    return BuildPipelineCodes.Canceled;
                }
            }

            output = PlayerBuildInterface.CompilePlayerScripts(settings, outputFolder);
            if (output.assemblies.IsNullOrEmpty() && output.typeDB == null)
                return BuildPipelineCodes.Error;

            if (progressTracker != null) // can't use null propagation
            {
                if (!progressTracker.EndProgress())
                    return BuildPipelineCodes.Canceled;
            }
            return BuildPipelineCodes.Success;
        }
    }
}
