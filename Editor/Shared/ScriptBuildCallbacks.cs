using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build
{
    public class ScriptBuildCallbacks : IScriptsCallback
    {
        public Func<IBuildParams, IResultInfo, BuildPipelineCodes> PostScriptsCallbacks { get; set; }

        public BuildPipelineCodes PostScripts(IBuildParams buildParams, IResultInfo resultInfo)
        {
            if (PostScriptsCallbacks != null)
                return PostScriptsCallbacks(buildParams, resultInfo);
            return BuildPipelineCodes.Success;
        }
    }
}
