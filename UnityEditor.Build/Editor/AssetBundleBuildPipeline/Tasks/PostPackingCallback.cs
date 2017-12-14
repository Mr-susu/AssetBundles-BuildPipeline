using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Tasks
{
    public class PostPackingCallback : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { typeof(IBuildParams), typeof(IDependencyInfo), typeof(IWriteInfo), typeof(IPackingCallback) };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IDependencyInfo>(),
                context.GetContextObject<IWriteInfo>(), context.GetContextObject<IPackingCallback>());
        }

        public BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo input1, IWriteInfo input2, IPackingCallback output)
        {
            return output.PostPacking(buildParams, input1, input2);
        }
    }
}
