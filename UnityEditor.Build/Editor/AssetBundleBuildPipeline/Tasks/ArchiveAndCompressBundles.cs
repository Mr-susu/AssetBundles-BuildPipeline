using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public class ArchiveAndCompressBundles : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IResultInfo>(), context.GetContextObject<IResultInfo>());
        }

        protected static Hash128 CalculateInputHash(bool useCache, ResourceFile[] resourceFiles, BuildCompression compression)
        {
            if (!useCache)
                return new Hash128();

            var fileHashes = new List<string>();
            foreach (ResourceFile file in resourceFiles)
                fileHashes.Add(HashingMethods.CalculateFileMD5Hash(file.fileName).ToString());
            return HashingMethods.CalculateMD5Hash(k_Version, fileHashes, compression);
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IResultInfo input, IResultInfo output)
        {
            foreach (KeyValuePair<string, List<WriteResult>> bundle in input.BundleResults)
            {
                ResourceFile[] resourceFiles = bundle.Value.SelectMany(x => x.resourceFiles).ToArray();
                Hash128 hash = CalculateInputHash(buildParams.UseCache, resourceFiles, buildParams.Compression);

                var finalPath = string.Format("{0}/{1}", buildParams.OutputFolder, bundle.Key);
                var writePath = string.Format("{0}/{1}", buildParams.GetTempOrCacheBuildPath(hash), bundle.Key);

                var bundleInfo = new BundleInfo();
                if (TryLoadFromCache(buildParams.UseCache, hash, ref bundleInfo))
                {
                    SetOutputInformation(writePath, finalPath, bundleInfo, output);
                    continue;
                }

                bundleInfo.name = bundle.Key;
                bundleInfo.crc = BundleBuildInterface.ArchiveAndCompress(resourceFiles, writePath, buildParams.Compression);
                bundleInfo.hash = HashingMethods.CalculateFileMD5Hash(writePath);
                SetOutputInformation(writePath, finalPath, bundleInfo, output);

                if (!TrySaveToCache(buildParams.UseCache, hash, bundleInfo))
                    BuildLogger.LogWarning("Unable to cache ArchiveAndCompressBundles result for bundle {0}.", bundle.Key);
            }

            return BuildPipelineCodes.Success;
        }

        protected static bool TryLoadFromCache(bool useCache, Hash128 hash, ref BundleInfo output)
        {
            BundleInfo cachedInfo;
            if (useCache && BuildCache.TryLoadCachedResults(hash, out cachedInfo))
            {
                output = cachedInfo;
                return true;
            }
            return false;
        }

        protected static bool TrySaveToCache(bool useCache, Hash128 hash, BundleInfo output)
        {
            if (useCache && !BuildCache.SaveCachedResults(hash, output))
                return false;
            return true;
        }

        protected static void SetOutputInformation(string writePath, string finalPath, BundleInfo bundleInfo, IResultInfo output)
        {
            if (finalPath != writePath)
            {
                var directory = Path.GetDirectoryName(finalPath);
                Directory.CreateDirectory(directory);
                File.Copy(writePath, finalPath, true);
            }
            output.BundleInfos.Add(finalPath, bundleInfo);
        }
    }
}
