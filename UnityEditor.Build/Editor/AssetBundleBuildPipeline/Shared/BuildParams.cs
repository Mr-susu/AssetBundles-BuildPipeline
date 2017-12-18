using System;
using System.IO;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEditor.Experimental.Build.Player;
using UnityEngine;

namespace UnityEditor.Build
{
    [Serializable]
    public class BuildParams : IBuildParams
    {
        public ScriptCompilationSettings ScriptSettings { get; protected set; }
        public BuildSettings BundleSettings { get; set; }
        public BuildCompression BundleCompression { get; protected set; }
        public string OutputFolder { get; protected set; }

        public string TempOutputFolder { get; protected set; }

        public bool UseCache { get; protected set; }

        public BuildParams(BuildSettings settings, BuildCompression compression, string outputFolder, string tempOutputFolder = null, bool useCache = false)
        {
            BundleSettings = settings;
            BundleCompression = compression;
            OutputFolder = outputFolder;
            if (useCache && string.IsNullOrEmpty(tempOutputFolder))
                throw new ArgumentException("Argument cannot be null or empty.", "tempOutputFolder");
            TempOutputFolder = tempOutputFolder;
            UseCache = useCache;
        }

        public BuildParams(ScriptCompilationSettings settings, string outputFolder, string tempOutputFolder = null, bool useCache = false)
        {
            ScriptSettings = settings;
            OutputFolder = outputFolder;
            if (useCache && string.IsNullOrEmpty(tempOutputFolder))
                throw new ArgumentException("Argument cannot be null or empty.", "tempOutputFolder");
            TempOutputFolder = tempOutputFolder;
            UseCache = useCache;
        }

        public BuildParams(ScriptCompilationSettings scriptSettings, BuildSettings bundleSettings, BuildCompression compression, string outputFolder, string tempOutputFolder = null, bool useCache = false)
        {
            ScriptSettings = scriptSettings;
            BundleSettings = bundleSettings;
            BundleCompression = compression;
            OutputFolder = outputFolder;
            if (useCache && string.IsNullOrEmpty(tempOutputFolder))
                throw new ArgumentException("Argument cannot be null or empty.", "tempOutputFolder");
            TempOutputFolder = tempOutputFolder;
            UseCache = useCache;
        }

        public string GetTempOrCacheBuildPath(Hash128 hash)
        {
            var path = TempOutputFolder;
            if (UseCache)
                path = BuildCache.GetPathForCachedArtifacts(hash);
            Directory.CreateDirectory(path);
            return path;
        }
    }
}