using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public struct ArchiveAndCompressBundles : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IBuildParams), typeof(IBundleWriteInfo), typeof(IBundleResults) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            IProgressTracker tracker;
            context.TryGetContextObject(out tracker);
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IBundleWriteInfo>(), context.GetContextObject<IBundleResults>(), tracker);
        }

        static Hash128 CalculateInputHash(bool useCache, ResourceFile[] resourceFiles, BuildCompression compression)
        {
            if (!useCache)
                return new Hash128();

            var fileHashes = new List<string>();
            foreach (ResourceFile file in resourceFiles)
                fileHashes.Add(HashingMethods.CalculateFileMD5Hash(file.fileName).ToString());
            return HashingMethods.CalculateMD5Hash(k_Version, fileHashes, compression);
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IBundleWriteInfo writeInfo, IBundleResults resultInfo, IProgressTracker tracker = null)
        {
            Dictionary<string, List<WriteResult>> bundleToResults = new Dictionary<string, List<WriteResult>>();
            foreach (var result in resultInfo.WriteResults)
            {
                var bundle = writeInfo.FileToBundle[result.Key];
                List<WriteResult> results;
                bundleToResults.GetOrAdd(bundle, out results);
                results.Add(result.Value);
            }

            foreach (KeyValuePair<string, List<WriteResult>> bundle in bundleToResults)
            {
                ResourceFile[] resourceFiles = bundle.Value.SelectMany(x => x.resourceFiles).ToArray();
                Hash128 hash = CalculateInputHash(buildParams.UseCache, resourceFiles, buildParams.BundleCompression);

                var finalPath = string.Format("{0}/{1}", buildParams.OutputFolder, bundle.Key);
                var writePath = string.Format("{0}/{1}", buildParams.GetTempOrCacheBuildPath(hash), bundle.Key);

                var bundleInfo = new BundleInfo();
                if (TryLoadFromCache(buildParams.UseCache, hash, ref bundleInfo))
                {
                    if (!tracker.UpdateInfoUnchecked(string.Format("{0} (Cached)", bundle.Key)))
                        return BuildPipelineCodes.Canceled;

                    SetOutputInformation(writePath, finalPath, bundle.Key, bundleInfo, resultInfo);
                    continue;
                }

                if (!tracker.UpdateInfoUnchecked(bundle.Key))
                    return BuildPipelineCodes.Canceled;

                bundleInfo.fileName = finalPath;
                bundleInfo.crc = BundleBuildInterface.ArchiveAndCompress(resourceFiles, writePath, buildParams.BundleCompression);
                bundleInfo.hash = HashingMethods.CalculateFileMD5Hash(writePath);
                SetOutputInformation(writePath, finalPath, bundle.Key, bundleInfo, resultInfo);

                if (!TrySaveToCache(buildParams.UseCache, hash, bundleInfo))
                    BuildLogger.LogWarning("Unable to cache ArchiveAndCompressBundles result for bundle {0}.", bundle.Key);
            }

            return BuildPipelineCodes.Success;
        }

        static bool TryLoadFromCache(bool useCache, Hash128 hash, ref BundleInfo output)
        {
            BundleInfo cachedInfo;
            if (useCache && BuildCache.TryLoadCachedResults(hash, out cachedInfo))
            {
                output = cachedInfo;
                return true;
            }
            return false;
        }

        static bool TrySaveToCache(bool useCache, Hash128 hash, BundleInfo output)
        {
            if (useCache && !BuildCache.SaveCachedResults(hash, output))
                return false;
            return true;
        }

        static void SetOutputInformation(string writePath, string finalPath, string bundleName, BundleInfo bundleInfo, IBundleResults output)
        {
            if (finalPath != writePath)
            {
                var directory = Path.GetDirectoryName(finalPath);
                Directory.CreateDirectory(directory);
                File.Copy(writePath, finalPath, true);
            }
            output.BundleInfos.Add(bundleName, bundleInfo);
        }
    }
}
