using System;
using System.IO;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public class DefaultBuildParams : IBuildParams
    {
        public BuildSettings Settings { get; protected set; }
        public BuildCompression Compression { get; protected set; }
        public string OutputFolder { get; protected set; }

        string m_TempOutputFolder;
        public string TempOutputFolder
        {
            get
            {
                if (UseCache)
                    return m_TempOutputFolder;
                return OutputFolder;
            }
            protected set
            {
                m_TempOutputFolder = value;
            }
        }

        public bool UseCache { get; protected set; }
        public IProgressTracker ProgressTracker { get; protected set; }

        public DefaultBuildParams(BuildSettings settings, BuildCompression compression, string outputFolder, string tempOutputFolder = null, bool useCache = false, IProgressTracker progressTracker = null)
        {
            Settings = settings;
            Compression = compression;
            OutputFolder = outputFolder;
            if (useCache && string.IsNullOrEmpty(tempOutputFolder))
                throw new ArgumentException("Argument cannot be null or empty.", "tempOutputFolder");
            TempOutputFolder = tempOutputFolder;
            UseCache = useCache;
            ProgressTracker = progressTracker;
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