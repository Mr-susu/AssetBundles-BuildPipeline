using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Tasks
{
    public class PostPackingCallback : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

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
