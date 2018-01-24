using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build;
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

        static Hash128 CalculateInputHash(bool useCache, GUID asset, BuildUsageTagGlobal globalUsage, BuildSettings settings)
        {
            if (!useCache)
                return new Hash128();

            string path = AssetDatabase.GUIDToAssetPath(asset.ToString());
            string assetHash = AssetDatabase.GetAssetDependencyHash(path).ToString();
            string[] dependencies = AssetDatabase.GetDependencies(path);
            var dependencyHashes = new string[dependencies.Length];
            for (int i = 0; i < dependencies.Length; ++i)
                dependencyHashes[i] = AssetDatabase.GetAssetDependencyHash(dependencies[i]).ToString();
            return HashingMethods.CalculateMD5Hash(k_Version, assetHash, dependencyHashes, globalUsage, settings);
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IBuildContent buildContent, IDependencyInfo dependencyInfo, IProgressTracker tracker = null)
        {
            var globalUsage = new BuildUsageTagGlobal();
            foreach (SceneDependencyInfo sceneInfo in dependencyInfo.SceneInfo.Values)
                globalUsage |= sceneInfo.globalUsage;

            foreach (GUID asset in buildContent.Assets)
            {
                var assetInfo = new AssetLoadInfo();
                var usageTags = new BuildUsageTagSet();
                string assetPath = AssetDatabase.GUIDToAssetPath(asset.ToString());

                Hash128 hash = CalculateInputHash(buildParams.UseCache, asset, globalUsage, buildParams.BundleSettings);
                if (TryLoadFromCache(buildParams.UseCache, hash, ref assetInfo, ref usageTags))
                {
                    if (!tracker.UpdateInfoUnchecked(string.Format("{0} (Cached)", assetPath)))
                        return BuildPipelineCodes.Canceled;

                    SetOutputInformation(asset, assetInfo, usageTags, dependencyInfo);
                    continue;
                }

                if (!tracker.UpdateInfoUnchecked(assetPath))
                    return BuildPipelineCodes.Canceled;

                assetInfo.asset = asset;
                assetInfo.address = buildContent.Addresses[asset];
                assetInfo.includedObjects = new List<ObjectIdentifier>(BundleBuildInterface.GetPlayerObjectIdentifiersInAsset(asset, buildParams.BundleSettings.target));

                var includedObjects = assetInfo.includedObjects.ToArray();
                assetInfo.referencedObjects = new List<ObjectIdentifier>(BundleBuildInterface.GetPlayerDependenciesForObjects(includedObjects, buildParams.BundleSettings.target, buildParams.BundleSettings.typeDB));
                BundleBuildInterface.CalculateBuildUsageTags(assetInfo.referencedObjects.ToArray(), includedObjects, globalUsage, usageTags);

                SetOutputInformation(asset, assetInfo, usageTags, dependencyInfo);

                if (!TrySaveToCache(buildParams.UseCache, hash, assetInfo, usageTags))
                    BuildLogger.LogWarning("Unable to cache AssetDependency results for asset '{0}'.", AssetDatabase.GUIDToAssetPath(asset.ToString()));
            }

            return BuildPipelineCodes.Success;
        }

        static void SetOutputInformation(GUID asset, AssetLoadInfo assetInfo, BuildUsageTagSet usageTags, IDependencyInfo output)
        {
            // Add generated asset information to BuildDependencyInfo
            output.AssetInfo.Add(asset, assetInfo);
            output.AssetUsage.Add(asset, usageTags);
        }

        static bool TryLoadFromCache(bool useCache, Hash128 hash, ref AssetLoadInfo assetInfo, ref BuildUsageTagSet usageTags)
        {
            AssetLoadInfo cachedAssetInfo;
            BuildUsageTagSet cachedAssetUsage;
            if (useCache && BuildCache.TryLoadCachedResults(hash, out cachedAssetInfo) && BuildCache.TryLoadCachedResults(hash, out cachedAssetUsage))
            {
                assetInfo = cachedAssetInfo;
                usageTags = cachedAssetUsage;
                return true;
            }

            return false;
        }

        static bool TrySaveToCache(bool useCache, Hash128 hash, AssetLoadInfo assetInfo, BuildUsageTagSet usageTags)
        {
            if (useCache && !(BuildCache.SaveCachedResults(hash, assetInfo) && BuildCache.SaveCachedResults(hash, usageTags)))
                return false;
            return true;
        }
    }
}
