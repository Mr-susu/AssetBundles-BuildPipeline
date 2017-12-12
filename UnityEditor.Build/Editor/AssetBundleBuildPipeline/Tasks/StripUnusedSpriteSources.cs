﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public class StripUnusedSpriteSources : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IDependencyInfo>(), context.GetContextObject<IDependencyInfo>());
        }

        protected static Hash128 CalculateInputHash(bool useCache, IDependencyInfo input)
        {
            if (!useCache)
                return new Hash128();

            return HashingMethods.CalculateMD5Hash(k_Version, input.AssetInfo, input.SceneInfo);
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo input, IDependencyInfo output)
        {
            var spriteSourceRef = new Dictionary<ObjectIdentifier, int>();

            Hash128 hash = CalculateInputHash(buildParams.UseCache, input);
            if (TryLoadFromCache(buildParams.UseCache, hash, ref spriteSourceRef))
            {
                SetOutputInformation(spriteSourceRef, output);
                return BuildPipelineCodes.SuccessCached;
            }

            // Create sprite source ref count map
            foreach (KeyValuePair<GUID, AssetLoadInfo> assetInfo in input.AssetInfo)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetInfo.Value.asset.ToString());
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType == TextureImporterType.Sprite && !string.IsNullOrEmpty(importer.spritePackingTag))
                    spriteSourceRef[assetInfo.Value.includedObjects[0]] = 0;
            }

            // Count refs from assets
            var assetRefs = input.AssetInfo.SelectMany(x => x.Value.referencedObjects);
            foreach (ObjectIdentifier reference in assetRefs)
            {
                int refCount = 0;
                if (!spriteSourceRef.TryGetValue(reference, out refCount))
                    continue;

                // Note: Because pass by value
                spriteSourceRef[reference] = ++refCount;
            }

            // Count refs from scenes
            var sceneRefs = input.SceneInfo.SelectMany(x => x.Value.referencedObjects);
            foreach (ObjectIdentifier reference in sceneRefs)
            {
                int refCount = 0;
                if (!spriteSourceRef.TryGetValue(reference, out refCount))
                    continue;

                // Note: Because pass by value
                spriteSourceRef[reference] = ++refCount;
            }

            SetOutputInformation(spriteSourceRef, output);

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