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
        public Dictionary<GUID, string> SceneAddress { get; private set; }
        public Dictionary<GUID, BuildUsageTagSet> SceneUsage { get; private set; }
        public BuildUsageTagGlobal GlobalUsage { get; set; }
        public Dictionary<GUID, List<string>> AssetToBundles { get; private set; }
        public Dictionary<string, List<GUID>> BundleToAssets { get; private set; }

        // TODO: Move this or something
        // Virtual assets for advanced object deduplication
        public HashSet<GUID> virtualAssets = new HashSet<GUID>();
        public Dictionary<ObjectIdentifier, GUID> objectToVirtualAsset = new Dictionary<ObjectIdentifier, GUID>();

        public BuildDependencyInfo()
        {
            AssetInfo = new Dictionary<GUID, AssetLoadInfo>();
            SceneInfo = new Dictionary<GUID, SceneDependencyInfo>();
            SceneAddress = new Dictionary<GUID, string>();
            GlobalUsage = new BuildUsageTagGlobal();
            SceneUsage = new Dictionary<GUID, BuildUsageTagSet>();
            AssetToBundles = new Dictionary<GUID, List<string>>();
            BundleToAssets = new Dictionary<string, List<GUID>>();
        }
    }
}
