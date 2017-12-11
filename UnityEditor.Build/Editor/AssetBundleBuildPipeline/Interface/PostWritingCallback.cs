namespace UnityEditor.Build
{
    public class PostWritingCallback : IBuildTask<IDependencyInfo, IWriteInfo, IResultInfo, IWritingCallback>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run((IDependencyInfo)context, (IWriteInfo)context, (IResultInfo)context, (IWritingCallback)context);
        }

        public BuildPipelineCodes Run(IDependencyInfo input1, IWriteInfo input2, IResultInfo input3, IWritingCallback output)
        {
            return output.PostWritingCallback(BuildParams, input1, input2, input3);
        }
    }
}
