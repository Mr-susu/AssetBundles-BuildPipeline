using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public struct CalculateAssetDependencyData : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IBuildParams), typeof(IBuildContent), typeof(IDependencyInfo) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            IProgressTracker tracker;
            context.TryGetContextObject(out tracker);
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IBuildContent>(), context.GetContextObject<IDependencyInfo>(), tracker);
        }

        static Hash128 CalculateInputHash(bool useCache, GUID asset, BuildSettings settings)
        {
            if (!useCache)
                return new Hash128();

            string path = AssetDatabase.GUIDToAssetPath(asset.ToString());
            string assetHash = AssetDatabase.GetAssetDependencyHash(path).ToString();
            string[] dependencies = AssetDatabase.GetDependencies(path);
            var dependencyHashes = new string[dependencies.Length];
            for (int i = 0; i < dependencies.Length; ++i)
                dependencyHashes[i] = AssetDatabase.GetAssetDependencyHash(dependencies[i]).ToString();
            return HashingMethods.CalculateMD5Hash(k_Version, assetHash, dependencyHashes, settings);
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IBuildContent input, IDependencyInfo output, IProgressTracker tracker = null)
        {
            foreach (GUID asset in input.Assets)
            {
                var assetInfo = new AssetLoadInfo();
                var assetPath = AssetDatabase.GUIDToAssetPath(asset.ToString());

                Hash128 hash = CalculateInputHash(buildParams.UseCache, asset, buildParams.BundleSettings);
                if (TryLoadFromCache(buildParams.UseCache, hash, ref assetInfo))
                {
                    if (!tracker.UpdateInfoUnchecked(string.Format("{0} (Cached)", assetPath)))
                        return BuildPipelineCodes.Canceled;

                    SetOutputInformation(asset, assetInfo, output);
                    continue;
                }

                if (!tracker.UpdateInfoUnchecked(assetPath))
                    return BuildPipelineCodes.Canceled;

                assetInfo.asset = asset;
                assetInfo.address = input.Addresses[asset];
                assetInfo.includedObjects = new List<ObjectIdentifier>(BundleBuildInterface.GetPlayerObjectIdentifiersInAsset(asset, buildParams.BundleSettings.target));
                assetInfo.referencedObjects = new List<ObjectIdentifier>(BundleBuildInterface.GetPlayerDependenciesForObjects(assetInfo.includedObjects.ToArray(), buildParams.BundleSettings.target, buildParams.BundleSettings.typeDB));

                SetOutputInformation(asset, assetInfo, output);

                if (!TrySaveToCache(buildParams.UseCache, hash, assetInfo))
                    BuildLogger.LogWarning("Unable to cache AssetDependency results for asset '{0}'.", AssetDatabase.GUIDToAssetPath(asset.ToString()));
            }

            return BuildPipelineCodes.Success;
        }

        static void SetOutputInformation(GUID asset, AssetLoadInfo assetInfo, IDependencyInfo output)
        {
            // Add generated asset information to BuildDependencyInfo
            output.AssetInfo.Add(asset, assetInfo);
        }

        static bool TryLoadFromCache(bool useCache, Hash128 hash, ref AssetLoadInfo assetInfo)
        {
            AssetLoadInfo cachedAssetInfo;
            if (useCache && BuildCache.TryLoadCachedResults(hash, out cachedAssetInfo))
            {
                assetInfo = cachedAssetInfo;
                return true;
            }

            return false;
        }

        static bool TrySaveToCache(bool useCache, Hash128 hash, AssetLoadInfo assetInfo)
        {
            if (useCache && !BuildCache.SaveCachedResults(hash, assetInfo))
                return false;
            return true;
        }
    }
}
