using System.Collections.Generic;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Interfaces
{
    public interface IDependencyInfo : IContextObject
    {
        // TODO: Revisit if these should be combined
        Dictionary<GUID, AssetLoadInfo> AssetInfo { get; }
        Dictionary<GUID, BuildUsageTagSet> AssetUsage { get; }

        Dictionary<GUID, SceneDependencyInfo> SceneInfo { get; }
        Dictionary<GUID, BuildUsageTagSet> SceneUsage { get; }
    }
}