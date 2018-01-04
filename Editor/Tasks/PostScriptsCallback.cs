using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Tasks
{
    public struct PostScriptsCallback : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IBuildParams), typeof(IResultInfo), typeof(IScriptsCallback) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IResultInfo>(), context.GetContextObject<IScriptsCallback>());
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IResultInfo buildResult, IScriptsCallback output)
        {
            return output.PostScripts(buildParams, buildResult);
        }
    }
}
