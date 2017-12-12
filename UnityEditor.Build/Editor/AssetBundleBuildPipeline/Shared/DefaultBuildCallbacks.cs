using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build
{
    public class DefaultBuildCallbacks : IDependencyCallback, IPackingCallback, IWritingCallback
    {
        public Func<IBuildParams, IDependencyInfo, BuildPipelineCodes> PostDependencyCallback { get; set; }
        public Func<IBuildParams, IDependencyInfo, IWriteInfo, BuildPipelineCodes> PostPackingCallback { get; set; }
        public Func<IBuildParams, IDependencyInfo, IWriteInfo, IResultInfo, BuildPipelineCodes> PostWritingCallback { get; set; }

        public BuildPipelineCodes PostDependency(IBuildParams buildParams, IDependencyInfo dependencyInfo)
        {
            if (PostDependencyCallback != null)
                return PostDependencyCallback(buildParams, dependencyInfo);
            return BuildPipelineCodes.Success;
        }

        public BuildPipelineCodes PostPacking(IBuildParams buildParams, IDependencyInfo dependencyInfo, IWriteInfo writeInfo)
        {
            if (PostPackingCallback != null)
                return PostPackingCallback(buildParams, dependencyInfo, writeInfo);
            return BuildPipelineCodes.Success;
        }

        public BuildPipelineCodes PostWriting(IBuildParams buildParams, IDependencyInfo dependencyInfo, IWriteInfo writeInfo, IResultInfo resultInfo)
        {
            if (PostWritingCallback != null)
                return PostWritingCallback(buildParams, dependencyInfo, writeInfo, resultInfo);
            return BuildPipelineCodes.Success;
        }
    }
}
