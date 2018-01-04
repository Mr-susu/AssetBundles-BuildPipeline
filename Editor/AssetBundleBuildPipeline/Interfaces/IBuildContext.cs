using System;

namespace UnityEditor.Build.Interfaces
{
    public interface IContextObject { }

    public interface IScriptsCallback : IContextObject
    {
        BuildPipelineCodes PostScripts(IBuildParams buildParams, IResultInfo resultInfo);
    }

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
        bool ContainsContextObject<T>() where T : IContextObject;
        bool ContainsContextObject(Type type);

        T GetContextObject<T>() where T : IContextObject;
        IContextObject GetContextObject(Type type);

        void SetContextObject<T>(IContextObject contextObject) where T : IContextObject;
        void SetContextObject(IContextObject contextObject);

        bool TryGetContextObject<T>(out T contextObject) where T : IContextObject;
    }
}
