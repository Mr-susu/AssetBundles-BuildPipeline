using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEditor.Experimental.Build.Player;

namespace UnityEditor.Build
{
    [Serializable]
    public class BuildResultInfo : IBundleResults
    {
        public ScriptCompilationResult ScriptResults { get; set; }
        public Dictionary<string, BundleInfo> BundleInfos { get; private set; }
        public Dictionary<string, WriteResult> WriteResults { get; private set; }

        public BuildResultInfo()
        {
            BundleInfos = new Dictionary<string, BundleInfo>();
            WriteResults = new Dictionary<string, WriteResult>();
        }
    }
}
