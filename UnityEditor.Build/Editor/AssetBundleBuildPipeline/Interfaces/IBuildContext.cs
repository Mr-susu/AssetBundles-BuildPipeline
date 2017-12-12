namespace UnityEditor.Build.Interfaces
{
    public interface IContextObject { }

    public interface IDependencyCallback : IContextObject
    {
        BuildPipelineCodes PostDependency(IBuildParams buildParams, IDependencyInfo dependencyInfo);
    }

    public interface IPackingCallback : IContextObject
    {
        BuildPipelineCodes PostPacking(IBuildParams buildParams, IDependencyInfo dependencyInfo, IWriteInfo writeInfo);
    }

    public interface IWritingCallback : IContextObject
    {
        BuildPipelineCodes PostWriting(IBuildParams buildParams, IDependencyInfo dependencyInfo, IWriteInfo writeInfo, IResultInfo resultInfo);
    }

    public interface IBuildContext
    {
        T GetContextObject<T>() where T : IContextObject;
        bool TryGetContextObject<T>(out IContextObject contextObject) where T : IContextObject;
    }
}
