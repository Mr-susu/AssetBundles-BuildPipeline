using System;
using System.Collections.Generic;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.AssetBundle
{
    [Serializable]
    public struct BundleInfo
    {
        public uint crc;
        public Hash128 hash;
    }

    [Serializable]
    public class BuildResultInfo
    {
        public Dictionary<string, BundleInfo> bundleInfos = new Dictionary<string, BundleInfo>();
        public Dictionary<string, List<WriteResult>> bundleResults = new Dictionary<string, List<WriteResult>>();
    }
}
