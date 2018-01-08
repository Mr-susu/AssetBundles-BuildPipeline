using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    [Serializable]
    public class BuildDependencyInfo : IDependencyInfo
    {
        public Dictionary<GUID, AssetLoadInfo> AssetInfo { get; private set; }
        public Dictionary<GUID, SceneDependencyInfo> SceneInfo { get; private set; }
        public Dictionary<GUID, BuildUsageTagSet> SceneUsage { get; private set; }
        public BuildUsageTagGlobal GlobalUsage { get; set; }

        public BuildDependencyInfo()
        {
            AssetInfo = new Dictionary<GUID, AssetLoadInfo>();
            SceneInfo = new Dictionary<GUID, SceneDependencyInfo>();
            GlobalUsage = new BuildUsageTagGlobal();
            SceneUsage = new Dictionary<GUID, BuildUsageTagSet>();
        }
    }
}
