using System.Collections.Generic;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Interfaces
{
    public interface IDependencyInfo : IContextObject
    {
        Dictionary<GUID, AssetLoadInfo> AssetInfo { get; }
        Dictionary<GUID, SceneDependencyInfo> SceneInfo { get; }
        // NOTES: Only used in Asset Bundle Compatible pipeline
        Dictionary<GUID, BuildUsageTagSet> SceneUsage { get; }
        BuildUsageTagGlobal GlobalUsage { get; set; }
        // NOTES: Only used in Auto-Packing pipelines
        BuildUsageTagSet BuildUsage { get; }
    }
}