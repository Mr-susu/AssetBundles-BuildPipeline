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
        public Dictionary<string, WriteResult> WriteResults { get; private set; }

        public BuildResultInfo()
        {
            WriteResults = new Dictionary<string, WriteResult>();
        }
    }

    [Serializable]
    public class BundleResultInfo : IBundleResultInfo
    {
        public ScriptCompilationResult ScriptResults { get; set; }
        public Dictionary<string, BundleInfo> BundleInfos { get; private set; }
        public Dictionary<string, WriteResult> WriteResults { get; private set; }

        public BundleResultInfo()
        {
            BundleInfos = new Dictionary<string, BundleInfo>();
            WriteResults = new Dictionary<string, WriteResult>();
        }
    }
}
