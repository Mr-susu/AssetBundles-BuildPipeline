using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEditor.Experimental.Build.Player;

namespace UnityEditor.Build
{
    [Serializable]
    public class BuildResultInfo : IResultInfo
    {
        public ScriptCompilationResult ScriptResults { get; set; }
        public Dictionary<string, BundleInfo> BundleInfos { get; private set; }
        public Dictionary<string, List<WriteResult>> BundleResults { get; private set; }

        public BuildResultInfo()
        {
            BundleInfos = new Dictionary<string, BundleInfo>();
            BundleResults = new Dictionary<string, List<WriteResult>>();
        }
    }
}
