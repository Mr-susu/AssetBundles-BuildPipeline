using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.AssetBundle;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public class ArchiveAndCompressBundles : IBuildTask<IResultInfo, IResultInfo>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run((IResultInfo)context, (IResultInfo)context);
        }

        protected Hash128 CalculateInputHash(List<ResourceFile> resourceFiles, BuildCompression compression)
        {
            if (!BuildParams.UseCache)
                return new Hash128();

            var fileHashes = new List<string>();
            foreach (var file in resourceFiles)
                fileHashes.Add(HashingMethods.CalculateFileMD5Hash(file.fileName).ToString());
            return HashingMethods.CalculateMD5Hash(Version, fileHashes, compression);
        }

        public BuildPipelineCodes Run(IResultInfo input, IResultInfo output)
        {
            foreach (var bundle in input.BundleResults)
            {
                var resourceFiles = new List<ResourceFile>(bundle.Value.SelectMany(x => x.resourceFiles));
                Hash128 hash = CalculateInputHash(resourceFiles, BuildParams.Compression);

                var finalPath = string.Format("{0}/{1}", BuildParams.OutputFolder, bundle.Key);
                var writePath = string.Format("{0}/{1}", BuildParams.GetTempOrCacheBuildPath(hash), bundle.Key);

                BundleInfo bundleInfo = new BundleInfo();
                if (TryLoadFromCache(hash, ref bundleInfo))
                {
                    SetOutputInformation(writePath, finalPath, bundleInfo, output);
                    continue;
                }

                bundleInfo.crc = BundleBuildInterface.ArchiveAndCompress(resourceFiles.ToArray(), writePath, BuildParams.Compression);
                bundleInfo.hash = HashingMethods.CalculateFileMD5Hash(writePath);
                SetOutputInformation(writePath, finalPath, bundleInfo, output);

                if (!TrySaveToCache(hash, bundleInfo))
                    BuildLogger.LogWarning("Unable to cache ArchiveAndCompressBundles result for bundle {0}.", bundle.Key);
            }

            return BuildPipelineCodes.Success;
        }

        protected bool TryLoadFromCache(Hash128 hash, ref BundleInfo output)
        {
            BundleInfo cachedInfo;
            if (BuildParams.UseCache && BuildCache.TryLoadCachedResults(hash, out cachedInfo))
            {
                output = cachedInfo;
                return true;
            }
            return false;
        }

        protected bool TrySaveToCache(Hash128 hash, BundleInfo output)
        {
            if (BuildParams.UseCache && !BuildCache.SaveCachedResults(hash, output))
                return false;
            return true;
        }

        protected void SetOutputInformation(string writePath, string finalPath, BundleInfo bundleInfo, IResultInfo output)
        {
            if (finalPath != writePath)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(finalPath));
                File.Copy(writePath, finalPath, true);
            }
            output.BundleInfos.Add(finalPath, bundleInfo);
        }
    }
}
