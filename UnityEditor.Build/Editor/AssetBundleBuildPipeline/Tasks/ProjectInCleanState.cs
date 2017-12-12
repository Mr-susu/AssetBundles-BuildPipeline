using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build.Tasks
{
    public class ProjectInCleanState : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run();
        }

        public static BuildPipelineCodes Run()
        {
            if (ProjectValidator.HasDirtyScenes())
                return BuildPipelineCodes.Success;
            return BuildPipelineCodes.UnsavedChanges;
        }
    }
}
