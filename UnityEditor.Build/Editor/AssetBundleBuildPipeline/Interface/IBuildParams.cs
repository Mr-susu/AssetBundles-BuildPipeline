using System;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public interface IBuildParams : IContextObject
    {
        BuildSettings Settings { get; }
        BuildCompression Compression { get; }
        string OutputFolder { get; }
        string TempOutputFolder { get; }
        bool UseCache { get; }
        IProgressTracker ProgressTracker { get; }

        string GetTempOrCacheBuildPath(Hash128 hash);
    }
}
