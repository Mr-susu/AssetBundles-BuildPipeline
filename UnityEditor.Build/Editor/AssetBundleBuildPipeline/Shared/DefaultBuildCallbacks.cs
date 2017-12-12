using System;

namespace UnityEditor.Build
{
    public class DefaultBuildCallbacks : IDependencyCallback, IPackingCallback, IWritingCallback
    {
        public Func<IBuildParams, IDependencyInfo, BuildPipelineCodes> PostDependencyCallback { get; set; }
        public Func<IBuildParams, IDependencyInfo, IWriteInfo, BuildPipelineCodes> PostPackingCallback { get; set; }
        public Func<IBuildParams, IDependencyInfo, IWriteInfo, IResultInfo, BuildPipelineCodes> PostWritingCallback { get; set; }
    }
}
