using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Tasks
{
    public class PostWritingCallback : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { typeof(IBuildParams), typeof(IDependencyInfo), typeof(IWriteInfo), typeof(IResultInfo), typeof(IWritingCallback) };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IDependencyInfo>(),
                context.GetContextObject<IWriteInfo>(), context.GetContextObject<IResultInfo>(), context.GetContextObject<IWritingCallback>());
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo input1, IWriteInfo input2, IResultInfo input3, IWritingCallback output)
        {
            return output.PostWriting(buildParams, input1, input2, input3);
        }
    }
}
