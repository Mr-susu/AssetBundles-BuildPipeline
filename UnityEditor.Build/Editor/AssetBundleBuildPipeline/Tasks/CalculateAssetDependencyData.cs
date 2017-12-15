using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public class CalculateAssetDependencyData : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { typeof(IBuildParams), typeof(IBuildLayout), typeof(IDependencyInfo) };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            IProgressTracker tracker;
            context.TryGetContextObject(out tracker);
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IBuildLayout>(), context.GetContextObject<IDependencyInfo>(), tracker);
        }

        protected static Hash128 CalculateInputHash(bool useCache, GUID asset, BuildSettings settings)
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

        public static BuildPipelineCodes Run(IBuildParams buildParams, IBuildLayout input, IDependencyInfo output, IProgressTracker tracker = null)
        {
            List<AssetIdentifier> assetIDs = input.Layout.definitions.SelectMany(x => x.explicitAssets).Where(x => ExtensionMethods.ValidAsset(x.asset)).ToList();
            foreach (AssetIdentifier assetID in assetIDs)
            {
                var assetInfo = new AssetLoadInfo();
                var assetPath = AssetDatabase.GUIDToAssetPath(assetID.asset.ToString());

                Hash128 hash = CalculateInputHash(buildParams.UseCache, assetID.asset, buildParams.BundleSettings);
                if (TryLoadFromCache(buildParams.UseCache, hash, ref assetInfo))
                {
                    if (!tracker.UpdateInfoUnchecked(string.Format("{0} (Cached)", assetPath)))
                        return BuildPipelineCodes.Canceled;

                    SetOutputInformation(assetID, assetInfo, output);
                    continue;
                }

                if (!tracker.UpdateInfoUnchecked(assetPath))
                    return BuildPipelineCodes.Canceled;

                assetInfo.asset = assetID.asset;
                assetInfo.address = string.IsNullOrEmpty(assetID.address) ? AssetDatabase.GUIDToAssetPath(assetID.asset.ToString()) : assetID.address;
                assetInfo.explicitDataLayout = true;
                assetInfo.includedObjects = new List<ObjectIdentifier>(BundleBuildInterface.GetPlayerObjectIdentifiersInAsset(assetID.asset, buildParams.BundleSettings.target));
                assetInfo.referencedObjects = new List<ObjectIdentifier>(BundleBuildInterface.GetPlayerDependenciesForObjects(assetInfo.includedObjects.ToArray(), buildParams.BundleSettings.target, buildParams.BundleSettings.typeDB));

                SetOutputInformation(assetID, assetInfo, output);

                if (!TrySaveToCache(buildParams.UseCache, hash, assetInfo))
                    BuildLogger.LogWarning("Unable to cache AssetDependency results for asset '{0}'.", assetID.asset);
            }

            return BuildPipelineCodes.Success;
        }

        protected static void SetOutputInformation(AssetIdentifier assetID, AssetLoadInfo assetInfo, IDependencyInfo output)
        {
            // Add generated asset information to BuildDependencyInfo
            output.AssetInfo.Add(assetID.asset, assetInfo);
        }

        protected static bool TryLoadFromCache(bool useCache, Hash128 hash, ref AssetLoadInfo assetInfo)
        {
            AssetLoadInfo cachedAssetInfo;
            if (useCache && BuildCache.TryLoadCachedResults(hash, out cachedAssetInfo))
            {
                assetInfo = cachedAssetInfo;
                return true;
            }

            return false;
        }

        protected static bool TrySaveToCache(bool useCache, Hash128 hash, AssetLoadInfo assetInfo)
        {
            if (useCache && !BuildCache.SaveCachedResults(hash, assetInfo))
                return false;
            return true;
        }
    }
}
