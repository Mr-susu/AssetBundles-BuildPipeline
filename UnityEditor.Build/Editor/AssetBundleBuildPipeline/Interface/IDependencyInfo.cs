using System.Collections.Generic;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    public interface IDependencyInfo : IContextObject
    {
        Dictionary<GUID, AssetLoadInfo> AssetInfo { get; }
        Dictionary<GUID, SceneDependencyInfo> SceneInfo { get; }
        Dictionary<GUID, string> SceneAddress { get; }
        Dictionary<GUID, BuildUsageTagSet> SceneUsage { get; }
        BuildUsageTagGlobal GlobalUsage { get; set; }
        Dictionary<GUID, List<string>> AssetToBundles { get; }
        Dictionary<string, List<GUID>> BundleToAssets { get; }
    }
}