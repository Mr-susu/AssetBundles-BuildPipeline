using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Tasks
{
    public class SetBundleSettingsTypeDB : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { typeof(IResultInfo), typeof(IBuildParams) };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

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
