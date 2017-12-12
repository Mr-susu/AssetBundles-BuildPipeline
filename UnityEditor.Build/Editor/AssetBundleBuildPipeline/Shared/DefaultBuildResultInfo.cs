using System;
using System.Collections.Generic;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.AssetBundle
{
    [Serializable]
    public class DefaultBuildResultInfo : IResultInfo
    {
        public Dictionary<string, BundleInfo> BundleInfos { get; private set; }
        public Dictionary<string, List<WriteResult>> BundleResults { get; private set; }

        public DefaultBuildResultInfo()
        {
            BundleInfos = new Dictionary<string, BundleInfo>();
            BundleResults = new Dictionary<string, List<WriteResult>>();
        }
    }
}
