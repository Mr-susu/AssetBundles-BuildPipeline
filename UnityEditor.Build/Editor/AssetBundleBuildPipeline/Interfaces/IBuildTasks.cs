namespace UnityEditor.Build.Interfaces
{
    public interface IBuildTask
    {
        int Version { get; }

        BuildPipelineCodes Run(IBuildContext context);
    }
}
