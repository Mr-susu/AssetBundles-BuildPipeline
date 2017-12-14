using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Tasks
{
    public class PostDependencyCallback : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { typeof(IBuildParams), typeof(IDependencyInfo), typeof(IDependencyCallback) };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IDependencyInfo>(),
                context.GetContextObject<IDependencyCallback>());
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo input, IDependencyCallback output)
        {
            return output.PostDependency(buildParams, input);
        }
    }
}
