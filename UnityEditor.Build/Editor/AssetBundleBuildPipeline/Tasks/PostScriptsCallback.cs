using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Tasks
{
    public class PostScriptsCallback : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { typeof(IBuildParams), typeof(IResultInfo), typeof(IScriptsCallback) };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

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
