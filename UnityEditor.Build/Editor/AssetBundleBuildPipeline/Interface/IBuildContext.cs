using System;

namespace UnityEditor.Build
{
    public interface IContextObject { }

    public interface IDependencyCallback : IContextObject
    {
        Func<IBuildParams, IDependencyInfo, BuildPipelineCodes> PostDependencyCallback { get; }
    }

    public interface IPackingCallback : IContextObject
    {
        Func<IBuildParams, IDependencyInfo, IWriteInfo, BuildPipelineCodes> PostPackingCallback { get; }
    }

    public interface IWritingCallback : IContextObject
    {
        Func<IBuildParams, IDependencyInfo, IWriteInfo, IResultInfo, BuildPipelineCodes> PostWritingCallback { get; }
    }

    public interface IBuildContext
    {
        T GetContextObject<T>() where T : IContextObject;
        bool TryGetContextObject<T>(out IContextObject contextObject) where T : IContextObject;
    }
}
