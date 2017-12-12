using System.Collections.Generic;
using UnityEditor.Build.AssetBundle;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    public interface IResultInfo : IContextObject
    {
        Dictionary<string, BundleInfo> BundleInfos { get; }
        Dictionary<string, List<WriteResult>> BundleResults { get; }
    }
}