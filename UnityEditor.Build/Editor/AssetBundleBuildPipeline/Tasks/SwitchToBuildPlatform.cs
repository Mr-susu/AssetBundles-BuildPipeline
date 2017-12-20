using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Tasks
{
    public struct SwitchToBuildPlatform : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IBuildParams) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>());
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams)
        {
            if (EditorUserBuildSettings.SwitchActiveBuildTarget(buildParams.BundleSettings.group, buildParams.BundleSettings.target))
                return BuildPipelineCodes.Success;
            return BuildPipelineCodes.Error;
        }
    }
}
