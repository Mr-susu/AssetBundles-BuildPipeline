using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public class StripUnusedSpriteSources : IBuildTask<IDependencyInfo, IDependencyInfo>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run((IDependencyInfo)context, (IDependencyInfo)context);
        }

        protected Hash128 CalculateInputHash(IDependencyInfo input)
        {
            if (!BuildParams.UseCache)
                return new Hash128();
            
            return HashingMethods.CalculateMD5Hash(Version, input.AssetInfo, input.SceneInfo);
        }

        public BuildPipelineCodes Run(IDependencyInfo input, IDependencyInfo output)
        {
            var spriteSourceRef = new Dictionary<ObjectIdentifier, int>();

            Hash128 hash = CalculateInputHash(input);
            if (TryLoadFromCache(hash, ref spriteSourceRef))
            {
                SetOutputInformation(spriteSourceRef, output);
                return BuildPipelineCodes.SuccessCached;
            }
            
            // Create sprite source ref count map
            foreach (var assetInfo in input.AssetInfo)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetInfo.Value.asset.ToString());
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType == TextureImporterType.Sprite && !string.IsNullOrEmpty(importer.spritePackingTag))
                    spriteSourceRef[assetInfo.Value.includedObjects[0]] = 0;
            }

            // Count refs from assets
            var assetRefs = input.AssetInfo.SelectMany(x => x.Value.referencedObjects);
            foreach (var reference in assetRefs)
            {
                int refCount = 0;
                if (!spriteSourceRef.TryGetValue(reference, out refCount))
                    continue;

                // Note: Because pass by value
                spriteSourceRef[reference] = ++refCount;
            }

            // Count refs from scenes
            var sceneRefs = input.SceneInfo.SelectMany(x => x.Value.referencedObjects);
            foreach (var reference in sceneRefs)
            {
                int refCount = 0;
                if (!spriteSourceRef.TryGetValue(reference, out refCount))
                    continue;

                // Note: Because pass by value
                spriteSourceRef[reference] = ++refCount;
            }

            SetOutputInformation(spriteSourceRef, output);

            return BuildPipelineCodes.Success;
        }

        protected void SetOutputInformation(Dictionary<ObjectIdentifier, int> spriteSourceRef, IDependencyInfo output)
        {
            foreach (var source in spriteSourceRef)
            {
                if (source.Value > 0)
                    continue;

                var assetInfo = output.AssetInfo[source.Key.guid];
                assetInfo.includedObjects.RemoveAt(0);
            }
        }

        protected bool TryLoadFromCache(Hash128 hash, ref Dictionary<ObjectIdentifier, int> spriteSourceRef)
        {
            Dictionary<ObjectIdentifier, int> cachedSpriteSourceRef;
            if (BuildParams.UseCache && BuildCache.TryLoadCachedResults(hash, out cachedSpriteSourceRef))
            {
                spriteSourceRef = cachedSpriteSourceRef;
                return true;
            }
            
            return false;
        }

        protected bool TrySaveToCache(Hash128 hash, Dictionary<ObjectIdentifier, int> spriteSourceRef)
        {
            if (BuildParams.UseCache && !BuildCache.SaveCachedResults(hash, spriteSourceRef))
                return true;
            return false;
        }
    }
}
