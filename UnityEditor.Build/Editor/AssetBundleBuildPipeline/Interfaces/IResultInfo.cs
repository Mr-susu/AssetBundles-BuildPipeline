using System.Collections.Generic;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEditor.Experimental.Build.Player;

namespace UnityEditor.Build.Interfaces
{
    public interface IResultInfo : IContextObject
    {
        ScriptCompilationResult ScriptResults { get; set; }
        Dictionary<string, BundleInfo> BundleInfos { get; }
        Dictionary<string, List<WriteResult>> BundleResults { get; }
    }
}