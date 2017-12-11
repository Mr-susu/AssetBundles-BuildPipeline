using System.IO;
using System.Linq;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public class CalculateSceneDependencyData : IBuildTask<IBundlesInput, IDependencyInfo>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        protected bool ValidScene(GUID asset)
        {
            var path = AssetDatabase.GUIDToAssetPath(asset.ToString());
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".unity") || !File.Exists(path))
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

        public BuildPipelineCodes Run(object context)
        {
            return Run((IBundlesInput)context, (IDependencyInfo)context);
        }

        public BuildPipelineCodes Run(IBundlesInput input, IDependencyInfo output)
        {
            var assetIDs = input.BundleInput.definitions.SelectMany(x => x.explicitAssets).Where(x => ValidScene(x.asset));
            if (BuildParams.ProgressTracker != null) // can't use null propagation
                BuildParams.ProgressTracker.StartStep("Processing Scene Dependencies", assetIDs.Count());

            foreach (AssetIdentifier assetID in assetIDs)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(assetID.asset.ToString());

                if (BuildParams.ProgressTracker != null) // can't use null propagation
                {
                    if (BuildParams.ProgressTracker.EndProgress())
                        return BuildPipelineCodes.Canceled;
                    BuildParams.ProgressTracker.UpdateProgress(scenePath);
                }

                var usageTags = new BuildUsageTagSet();
                var sceneInfo = new SceneDependencyInfo();

                Hash128 hash = CalculateInputHash(assetID.asset, BuildParams.Settings);
                if (TryLoadFromCache(hash, ref sceneInfo, ref usageTags))
                {
                    SetOutputInformation(assetID, sceneInfo, usageTags, output);
                    continue;
                }

                sceneInfo = BundleBuildInterface.PrepareScene(scenePath, BuildParams.Settings, usageTags, BuildParams.GetTempOrCacheBuildPath(hash));
                SetOutputInformation(assetID, sceneInfo, usageTags, output);

                if (!TrySaveToCache(hash, sceneInfo, usageTags))
                    BuildLogger.LogWarning("Unable to cache SceneDependency results for asset '{0}'.", assetID.asset);
            }

            if (BuildParams.ProgressTracker != null && BuildParams.ProgressTracker.EndProgress())
                return BuildPipelineCodes.Canceled;
            return BuildPipelineCodes.Success;
        }

        protected void SetOutputInformation(AssetIdentifier assetID, SceneDependencyInfo sceneInfo, BuildUsageTagSet usageTags, IDependencyInfo output)
        {
            // Add generated scene information to BuildDependencyInfo
            output.SceneInfo.Add(assetID.asset, sceneInfo);
            output.SceneUsage.Add(assetID.asset, usageTags);
            output.SceneAddress.Add(assetID.asset, assetID.address);
            output.GlobalUsage |= sceneInfo.globalUsage;
        }

        protected bool TryLoadFromCache(Hash128 hash, ref SceneDependencyInfo sceneInfo, ref BuildUsageTagSet usageTags)
        {
            SceneDependencyInfo cachedSceneInfo;
            BuildUsageTagSet cachedUsageTags;
            if (BuildParams.UseCache && BuildCache.TryLoadCachedResults(hash, out cachedSceneInfo) && BuildCache.TryLoadCachedResults(hash, out cachedUsageTags))
            {
                sceneInfo = cachedSceneInfo;
                usageTags = cachedUsageTags;
                return true;
            }
            
            return false;
        }

        protected bool TrySaveToCache(Hash128 hash, SceneDependencyInfo sceneInfo, BuildUsageTagSet usageTags)
        {
            if (BuildParams.UseCache && !BuildCache.SaveCachedResults(hash, sceneInfo) && !BuildCache.SaveCachedResults(hash, usageTags))
                return false;
            return true;
        }
    }
}
