namespace UnityEditor.Build
{
    public class PostDependencyCallback : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IDependencyInfo>(),
                context.GetContextObject<IDependencyCallback>());
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo input, IDependencyCallback output)
        {
            return output.PostDependencyCallback(buildParams, input);
        }
    }
}
