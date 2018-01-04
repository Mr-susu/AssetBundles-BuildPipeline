using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Tasks
{
    public struct SetBundleSettingsTypeDB : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IResultInfo), typeof(IBuildParams) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IResultInfo>(), context.GetContextObject<IBuildParams>());
        }

        public static BuildPipelineCodes Run(IResultInfo buildResult, IBuildParams buildParams)
        {
            var bundleSettings = buildParams.BundleSettings;
            bundleSettings.typeDB = buildResult.ScriptResults.typeDB;
            buildParams.BundleSettings = bundleSettings;
            return BuildPipelineCodes.Success;
        }
    }
}
