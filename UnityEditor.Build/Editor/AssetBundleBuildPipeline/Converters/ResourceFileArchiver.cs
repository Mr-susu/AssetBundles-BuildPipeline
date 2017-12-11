using System.Collections.Generic;
using System.IO;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

using BundleInfoMap = System.Collections.Generic.Dictionary<string, UnityEditor.Build.AssetBundle.BundleInfo>;
using System.Linq;

namespace UnityEditor.Build.AssetBundle.DataConverters
{
    public class ResourceFileArchiver : ADataConverter<BuildResultInfo, BuildCompression, string, BundleInfoMap>
    {
        public override uint Version { get { return 1; } }

        public ResourceFileArchiver(bool useCache, IProgressTracker progressTracker) : base(useCache, progressTracker) { }

        private Hash128 CalculateInputHash(List<ResourceFile> resourceFiles, BuildCompression compression)
        {
            if (!UseCache)
                return new Hash128();

            var fileHashes = new List<string>();
            foreach (var file in resourceFiles)
                fileHashes.Add(HashingMethods.CalculateFileMD5Hash(file.fileName).ToString());
            return HashingMethods.CalculateMD5Hash(Version, fileHashes, compression);
        }

        public override BuildPipelineCodes Convert(BuildResultInfo resultInfo, BuildCompression compression, string outputFolder, out BundleInfoMap output)
        {
            StartProgressBar("Archiving Resource Files", resultInfo.bundleResults.Count);
            output = new BundleInfoMap();

            foreach (var bundle in resultInfo.bundleResults)
            {
                if (!UpdateProgressBar(string.Format("Bundle: {0}", bundle.Key)))
                {
                    EndProgressBar();
                    return BuildPipelineCodes.Canceled;
                }

                var resourceFiles = new List<ResourceFile>(bundle.Value.SelectMany(x => x.resourceFiles));

                var filePath = string.Format("{0}/{1}", outputFolder, bundle.Key);

                BundleInfo bundleInfo;
                Hash128 hash = CalculateInputHash(resourceFiles, compression);
                if (UseCache && TryLoadFromCache(hash, outputFolder, out bundleInfo))
                {
                    output[filePath] = bundleInfo;
                    continue;
                }

                bundleInfo.crc = BundleBuildInterface.ArchiveAndCompress(resourceFiles.ToArray(), filePath, compression);
                bundleInfo.hash = HashingMethods.CalculateFileMD5Hash(filePath);
                output[filePath] = bundleInfo;

                if (UseCache && !TrySaveToCache(hash, bundle.Key, bundleInfo, outputFolder))
                    BuildLogger.LogWarning("Unable to cache ResourceFileArchiver result for bundle {0}.", bundle.Key);
            }

            if (!EndProgressBar())
                return BuildPipelineCodes.Canceled;
            return BuildPipelineCodes.Success;
        }

        private bool TryLoadFromCache(Hash128 hash, string outputFolder, out BundleInfo output)
        {
            string rootCachePath;
            string[] artifactPaths;

            if (!BuildCache.TryLoadCachedResultsAndArtifacts(hash, out output, out artifactPaths, out rootCachePath))
                return false;

            Directory.CreateDirectory(outputFolder);

            foreach (var artifact in artifactPaths)
                File.Copy(artifact, artifact.Replace(rootCachePath, outputFolder), true);
            return true;
        }

        private bool TrySaveToCache(Hash128 hash, string filePath, BundleInfo output, string outputFolder)
        {
            return BuildCache.SaveCachedResultsAndArtifacts(hash, output, new[] { filePath }, outputFolder);
        }
    }
}
