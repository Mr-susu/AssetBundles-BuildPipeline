namespace UnityEditor.Build
{
    public interface IBuildTask
    {
        int Version { get; }

        BuildPipelineCodes Run(IBuildContext context);
    }
}
