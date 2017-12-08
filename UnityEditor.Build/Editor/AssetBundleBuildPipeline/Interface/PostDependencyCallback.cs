namespace UnityEditor.Build
{
    public class PostDependencyCallback : IBuildTask<IDependencyInfo, IDependencyCallback>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run((IDependencyInfo)context, (IDependencyCallback)context);
        }

        public BuildPipelineCodes Run(IDependencyInfo input, IDependencyCallback output)
        {
            return output.PostDependencyCallback(BuildParams, input);
        }
    }
}
