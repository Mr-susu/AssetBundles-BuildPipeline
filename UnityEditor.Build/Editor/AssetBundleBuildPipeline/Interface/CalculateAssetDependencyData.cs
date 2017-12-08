using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public class CalculateAssetDependencyData : IBuildTask<IBundlesInput, IDependencyInfo>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run((IBundlesInput)context, (IDependencyInfo)context);
        }

        protected bool ValidAsset(GUID asset)
        {
            var path = AssetDatabase.GUIDToAssetPath(asset.ToString());
            if (string.IsNullOrEmpty(path) || path.EndsWith(".unity") || !File.Exists(path))
                return false;
            return true;
        }

        protected Hash128 CalculateInputHash(GUID asset, BuildSettings settings)
        {
            if (!BuildParams.UseCache)
                return new Hash128();

            var path = AssetDatabase.GUIDToAssetPath(asset.ToString());
            var assetHash = AssetDatabase.GetAssetDependencyHash(path).ToString();
            var dependencies = AssetDatabase.GetDependencies(path);
            var dependencyHashes = new string[dependencies.Length];
            for (var i = 0; i < dependencies.Length; ++i)
                dependencyHashes[i] = AssetDatabase.GetAssetDependencyHash(dependencies[i]).ToString();
            return HashingMethods.CalculateMD5Hash(Version, assetHash, dependencyHashes, settings);
        }

        public BuildPipelineCodes Run(IBundlesInput input, IDependencyInfo output)
        {
            var assetIDs = input.BundleInput.definitions.SelectMany(x => x.explicitAssets).Where(x => ValidAsset(x.asset));
            if (BuildParams.ProgressTracker != null) // can't use null propagation
                BuildParams.ProgressTracker.StartStep("Processing Asset Dependencies", assetIDs.Count());

            foreach (var assetID in assetIDs)
            {
               if (BuildParams.ProgressTracker != null) // can't use null propagation
                {
                    if (BuildParams.ProgressTracker.EndProgress())
                        return BuildPipelineCodes.Canceled;
                    string assetPath = AssetDatabase.GUIDToAssetPath(assetID.asset.ToString());
                    BuildParams.ProgressTracker.UpdateProgress(assetPath);
                }
                
                var assetInfo = new AssetLoadInfo();

                Hash128 hash = CalculateInputHash(assetID.asset, BuildParams.Settings);
                if (TryLoadFromCache(hash, ref assetInfo))
                {
                    SetOutputInformation(assetID, assetInfo, output);
                    continue;
                }

                assetInfo.asset = assetID.asset;
                assetInfo.address = string.IsNullOrEmpty(assetID.address) ? AssetDatabase.GUIDToAssetPath(assetID.asset.ToString()) : assetID.address;
                assetInfo.explicitDataLayout = true;
                assetInfo.includedObjects = new List<ObjectIdentifier>(BundleBuildInterface.GetPlayerObjectIdentifiersInAsset(assetID.asset, BuildParams.Settings.target));
                assetInfo.referencedObjects = new List<ObjectIdentifier>(BundleBuildInterface.GetPlayerDependenciesForObjects(assetInfo.includedObjects.ToArray(), BuildParams.Settings.target, BuildParams.Settings.typeDB));
                
                SetOutputInformation(assetID, assetInfo, output);

                if (TrySaveToCache(hash, assetInfo))
                    BuildLogger.LogWarning("Unable to cache AssetDependency results for asset '{0}'.", assetID.asset);
            }
            
            if (BuildParams.ProgressTracker != null && BuildParams.ProgressTracker.EndProgress())
                return BuildPipelineCodes.Canceled;
            return BuildPipelineCodes.Success;
        }

        protected void SetOutputInformation(AssetIdentifier assetID, AssetLoadInfo assetInfo, IDependencyInfo output)
        {
            // Add generated asset information to BuildDependencyInfo
            output.AssetInfo.Add(assetID.asset, assetInfo);
        }

        protected bool TryLoadFromCache(Hash128 hash, ref AssetLoadInfo assetInfo)
        {
            AssetLoadInfo cachedAssetInfo;
            if (BuildParams.UseCache && BuildCache.TryLoadCachedResults(hash, out cachedAssetInfo))
            {
                assetInfo = cachedAssetInfo;
                return true;
            }
            
            return false;
        }

        protected bool TrySaveToCache(Hash128 hash, AssetLoadInfo assetInfo)
        {
            if (BuildParams.UseCache && !BuildCache.SaveCachedResults(hash, assetInfo))
                return true;
            return false;
        }
    }
}
