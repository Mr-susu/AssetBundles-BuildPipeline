using System;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build.Tasks
{
    public struct ProjectInCleanState : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

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
