using System;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public struct CalculateSceneDependencyData : IBuildTask
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
            foreach (GUID asset in input.Scenes)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(asset.ToString());

                var usageTags = new BuildUsageTagSet();
                var sceneInfo = new SceneDependencyInfo();

                Hash128 hash = CalculateInputHash(buildParams.UseCache, asset, buildParams.BundleSettings);
                if (TryLoadFromCache(buildParams.UseCache, hash, ref sceneInfo, ref usageTags))
                {
                    if (!tracker.UpdateInfoUnchecked(string.Format("{0} (Cached)", scenePath)))
                        return BuildPipelineCodes.Canceled;

                    SetOutputInformation(asset, sceneInfo, usageTags, output);
                    continue;
                }

                if (!tracker.UpdateInfoUnchecked(scenePath))
                    return BuildPipelineCodes.Canceled;

                sceneInfo = BundleBuildInterface.PrepareScene(scenePath, buildParams.BundleSettings, usageTags, buildParams.GetTempOrCacheBuildPath(hash));
                SetOutputInformation(asset, sceneInfo, usageTags, output);

                if (!TrySaveToCache(buildParams.UseCache, hash, sceneInfo, usageTags))
                    BuildLogger.LogWarning("Unable to cache SceneDependency results for asset '{0}'.", AssetDatabase.GUIDToAssetPath(asset.ToString()));
            }

            return BuildPipelineCodes.Success;
        }

        static void SetOutputInformation(GUID asset, SceneDependencyInfo sceneInfo, BuildUsageTagSet usageTags, IDependencyInfo output)
        {
            // Add generated scene information to BuildDependencyInfo
            output.SceneInfo.Add(asset, sceneInfo);
            output.SceneUsage.Add(asset, usageTags);
            output.GlobalUsage |= sceneInfo.globalUsage;
        }

        static bool TryLoadFromCache(bool useCache, Hash128 hash, ref SceneDependencyInfo sceneInfo, ref BuildUsageTagSet usageTags)
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

        static bool TrySaveToCache(bool useCache, Hash128 hash, SceneDependencyInfo sceneInfo, BuildUsageTagSet usageTags)
        {
            if (useCache && !BuildCache.SaveCachedResults(hash, sceneInfo) && !BuildCache.SaveCachedResults(hash, usageTags))
                return false;
            return true;
        }
    }
}
