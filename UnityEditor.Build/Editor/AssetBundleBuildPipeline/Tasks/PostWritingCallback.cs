namespace UnityEditor.Build
{
    public class PostWritingCallback : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IDependencyInfo>(),
                context.GetContextObject<IWriteInfo>(), context.GetContextObject<IResultInfo>(), context.GetContextObject<IWritingCallback>());
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo input1, IWriteInfo input2, IResultInfo input3, IWritingCallback output)
        {
            return output.PostWritingCallback(buildParams, input1, input2, input3);
        }
    }
}
