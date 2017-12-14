using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEditor.Experimental.Build.Player;
using UnityEngine;

namespace UnityEditor.Build.Interfaces
{
    public interface IBuildParams : IContextObject
    {
        ScriptCompilationSettings ScriptSettings { get; }
        BuildSettings BundleSettings { get; set; }
        BuildCompression BundleCompression { get; }
        string OutputFolder { get; }
        string TempOutputFolder { get; }
        bool UseCache { get; }

        string GetTempOrCacheBuildPath(Hash128 hash);
    }
}
