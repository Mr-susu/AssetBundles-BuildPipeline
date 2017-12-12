using System.Collections.Generic;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Interfaces
{
    public interface IResultInfo : IContextObject
    {
        Dictionary<string, BundleInfo> BundleInfos { get; }
        Dictionary<string, List<WriteResult>> BundleResults { get; }
    }
}