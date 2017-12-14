using System;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build.Tasks
{
    public class ProjectInCleanState : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run();
        }

        public static BuildPipelineCodes Run()
        {
            if (ProjectValidator.HasDirtyScenes())
                return BuildPipelineCodes.UnsavedChanges;
            return BuildPipelineCodes.Success;
        }
    }
}
