using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public class CalculateSceneDependencyData : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IBundleInput>(), context.GetContextObject<IDependencyInfo>());
        }

        protected static bool ValidScene(GUID asset)
        {
            string path = AssetDatabase.GUIDToAssetPath(asset.ToString());
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".unity") || !File.Exists(path))
                return false;
            return true;
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

        public BuildPipelineCodes Run(IBuildParams buildParams, IBundleInput input, IDependencyInfo output)
        {
            List<AssetIdentifier> assetIDs = input.BundleInput.definitions.SelectMany(x => x.explicitAssets).Where(x => ValidScene(x.asset)).ToList();
            if (buildParams.ProgressTracker != null) // can't use null propagation yet
                buildParams.ProgressTracker.StartStep("Processing Scene Dependencies", assetIDs.Count());

            foreach (AssetIdentifier assetID in assetIDs)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(assetID.asset.ToString());

                if (buildParams.ProgressTracker != null) // can't use null propagation yet
                {
                    if (buildParams.ProgressTracker.EndProgress())
                        return BuildPipelineCodes.Canceled;
                    buildParams.ProgressTracker.UpdateProgress(scenePath);
                }

                var usageTags = new BuildUsageTagSet();
                var sceneInfo = new SceneDependencyInfo();

                Hash128 hash = CalculateInputHash(buildParams.UseCache, assetID.asset, buildParams.Settings);
                if (TryLoadFromCache(buildParams.UseCache, hash, ref sceneInfo, ref usageTags))
                {
                    SetOutputInformation(assetID, sceneInfo, usageTags, output);
                    continue;
                }

                sceneInfo = BundleBuildInterface.PrepareScene(scenePath, buildParams.Settings, usageTags, buildParams.GetTempOrCacheBuildPath(hash));
                SetOutputInformation(assetID, sceneInfo, usageTags, output);

                if (!TrySaveToCache(buildParams.UseCache, hash, sceneInfo, usageTags))
                    BuildLogger.LogWarning("Unable to cache SceneDependency results for asset '{0}'.", assetID.asset);
            }

            if (buildParams.ProgressTracker != null && buildParams.ProgressTracker.EndProgress())
                return BuildPipelineCodes.Canceled;
            return BuildPipelineCodes.Success;
        }

        protected static void SetOutputInformation(AssetIdentifier assetID, SceneDependencyInfo sceneInfo, BuildUsageTagSet usageTags, IDependencyInfo output)
        {
            // Add generated scene information to DefaultBuildDependencyInfo
            output.SceneInfo.Add(assetID.asset, sceneInfo);
            output.SceneUsage.Add(assetID.asset, usageTags);
            output.SceneAddress.Add(assetID.asset, assetID.address);
            output.GlobalUsage |= sceneInfo.globalUsage;
        }

        protected static bool TryLoadFromCache(bool useCache, Hash128 hash, ref SceneDependencyInfo sceneInfo, ref BuildUsageTagSet usageTags)
        {
            SceneDependencyInfo cachedSceneInfo;
            BuildUsageTagSet cachedUsageTags;
            if (useCache && BuildCache.TryLoadCachedResults(hash, out cachedSceneInfo) && BuildCache.TryLoadCachedResults(hash, out cachedUsageTags))
            {
                sceneInfo = cachedSceneInfo;
                usageTags = cachedUsageTags;
                return true;
            }

            return false;
        }

        protected bool TrySaveToCache(bool useCache, Hash128 hash, SceneDependencyInfo sceneInfo, BuildUsageTagSet usageTags)
        {
            if (useCache && !BuildCache.SaveCachedResults(hash, sceneInfo) && !BuildCache.SaveCachedResults(hash, usageTags))
                return false;
            return true;
        }
    }
}
