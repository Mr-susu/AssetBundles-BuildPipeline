using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public class StripUnusedSpriteSources : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { typeof(IResultInfo), typeof(IDependencyInfo), typeof(IDependencyInfo) };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IDependencyInfo>());
        }

        protected static Hash128 CalculateInputHash(bool useCache, IDependencyInfo input)
        {
            if (!useCache)
                return new Hash128();

            return HashingMethods.CalculateMD5Hash(k_Version, input.AssetInfo, input.SceneInfo);
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo dependencyInfo)
        {
            var spriteSourceRef = new Dictionary<ObjectIdentifier, int>();

            Hash128 hash = CalculateInputHash(buildParams.UseCache, dependencyInfo);
            if (TryLoadFromCache(buildParams.UseCache, hash, ref spriteSourceRef))
            {
                SetOutputInformation(spriteSourceRef, dependencyInfo);
                return BuildPipelineCodes.SuccessCached;
            }

            // CreateBundle sprite source ref count map
            foreach (KeyValuePair<GUID, AssetLoadInfo> assetInfo in dependencyInfo.AssetInfo)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetInfo.Value.asset.ToString());
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType == TextureImporterType.Sprite && !string.IsNullOrEmpty(importer.spritePackingTag))
                    spriteSourceRef[assetInfo.Value.includedObjects[0]] = 0;
            }

            // Count refs from assets
            var assetRefs = dependencyInfo.AssetInfo.SelectMany(x => x.Value.referencedObjects);
            foreach (ObjectIdentifier reference in assetRefs)
            {
                int refCount = 0;
                if (!spriteSourceRef.TryGetValue(reference, out refCount))
                    continue;

                // Note: Because pass by value
                spriteSourceRef[reference] = ++refCount;
            }

            // Count refs from scenes
            var sceneRefs = dependencyInfo.SceneInfo.SelectMany(x => x.Value.referencedObjects);
            foreach (ObjectIdentifier reference in sceneRefs)
            {
                int refCount = 0;
                if (!spriteSourceRef.TryGetValue(reference, out refCount))
                    continue;

                // Note: Because pass by value
                spriteSourceRef[reference] = ++refCount;
            }

            SetOutputInformation(spriteSourceRef, dependencyInfo);

            if (!TrySaveToCache(buildParams.UseCache, hash, spriteSourceRef))
                BuildLogger.LogWarning("Unable to cache StripUnusedSpriteSources results.");

            return BuildPipelineCodes.Success;
        }

        protected static void SetOutputInformation(Dictionary<ObjectIdentifier, int> spriteSourceRef, IDependencyInfo output)
        {
            foreach (KeyValuePair<ObjectIdentifier, int> source in spriteSourceRef)
            {
                if (source.Value > 0)
                    continue;

                var assetInfo = output.AssetInfo[source.Key.guid];
                assetInfo.includedObjects.RemoveAt(0);
            }
        }

        protected static bool TryLoadFromCache(bool useCache, Hash128 hash, ref Dictionary<ObjectIdentifier, int> spriteSourceRef)
        {
            Dictionary<ObjectIdentifier, int> cachedSpriteSourceRef;
            if (useCache && BuildCache.TryLoadCachedResults(hash, out cachedSpriteSourceRef))
            {
                spriteSourceRef = cachedSpriteSourceRef;
                return true;
            }

            return false;
        }

        protected static bool TrySaveToCache(bool useCache, Hash128 hash, Dictionary<ObjectIdentifier, int> spriteSourceRef)
        {
            if (useCache && !BuildCache.SaveCachedResults(hash, spriteSourceRef))
                return true;
            return false;
        }
    }
}
