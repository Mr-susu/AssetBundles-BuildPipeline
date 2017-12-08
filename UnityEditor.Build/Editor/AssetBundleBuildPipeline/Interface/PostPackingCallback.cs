namespace UnityEditor.Build
{
    public class PostPackingCallback : IBuildTask<IDependencyInfo, IWriteInfo, IPackingCallback>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run((IDependencyInfo)context, (IWriteInfo)context, (IPackingCallback)context);
        }

        public BuildPipelineCodes Run(IDependencyInfo input1, IWriteInfo input2, IPackingCallback output)
        {
            return output.PostPackingCallback(BuildParams, input1, input2);
        }
    }
}
