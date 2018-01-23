using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;

namespace Assets.UnityPackages.BuildPipeline.Editor.Tasks
{
    public class GenerateBundlePacking : IBuildTask
    {
        // TODO: Move to utility file
        public const string k_UnityDefaultResourcePath = "library/unity default resources";
        public const string k_AssetBundleNameFormat = "archive:/{0}/{0}";
        public const string k_SceneBundleNameFormat = "archive:/{0}/{1}.sharedAssets";

        const int k_Version = 1;

        public int Version
        {
            get { return k_Version; }
        }

        static readonly Type[] k_RequiredTypes = { typeof(IBundleContent), typeof(IDependencyInfo), typeof(IBundleWriteInfo), typeof(IDeterministicIdentifiers) };

        public Type[] RequiredContextTypes
        {
            get { return k_RequiredTypes; }
        }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBundleContent>(), context.GetContextObject<IDependencyInfo>(), context.GetContextObject<IBundleWriteInfo>(),
                context.GetContextObject<IDeterministicIdentifiers>());
        }

        public static BuildPipelineCodes Run(IBundleContent bundleContent, IDependencyInfo dependencyInfo, IBundleWriteInfo writeInfo, IDeterministicIdentifiers packingMethod)
        {
            Dictionary<GUID, List<GUID>> AssetToReferences = new Dictionary<GUID, List<GUID>>();

            // Pack each asset bundle
            foreach (var bundle in bundleContent.ExplicitLayout)
            {
                if (ExtensionMethods.ValidAssetBundle(bundle.Value))
                    PackAssetBundle(bundle.Key, bundle.Value, dependencyInfo, writeInfo, packingMethod, AssetToReferences);
                else if (ExtensionMethods.ValidSceneBundle(bundle.Value))
                    PackSceneBundle(bundle.Key, bundle.Value, dependencyInfo, writeInfo, packingMethod, AssetToReferences);
            }

            // Calculate Asset file load dependency list
            foreach (var bundle in bundleContent.ExplicitLayout)
            {
                foreach (var asset in bundle.Value)
                {
                    List<string> files = writeInfo.AssetToFiles[asset];
                    List<GUID> references = AssetToReferences[asset];
                    foreach (var reference in references)
                    {
                        List<string> referenceFiles = writeInfo.AssetToFiles[reference];
                        if (!files.Contains(referenceFiles[0]))
                            files.Add(referenceFiles[0]);
                    }
                }
            }

            return BuildPipelineCodes.Success;
        }

        static void PackAssetBundle(string bundleName, List<GUID> includedAssets, IDependencyInfo dependencyInfo, IBundleWriteInfo writeInfo, IDeterministicIdentifiers packingMethod, Dictionary<GUID, List<GUID>> assetToReferences)
        {
            var internalName = string.Format(k_AssetBundleNameFormat, packingMethod.GenerateInternalFileName(bundleName));

            var allObjects = new HashSet<ObjectIdentifier>();
            foreach (var asset in includedAssets)
            {
                AssetLoadInfo assetInfo = dependencyInfo.AssetInfo[asset];
                allObjects.UnionWith(assetInfo.includedObjects);

                var references = new List<ObjectIdentifier>();
                references.AddRange(assetInfo.referencedObjects);
                assetToReferences[asset] = FilterReferencesForAsset(dependencyInfo, asset, references);

                allObjects.UnionWith(references);
                writeInfo.AssetToFiles[asset] = new List<string> { internalName };
            }

            writeInfo.FileToBundle.Add(internalName, bundleName);
            writeInfo.FileToObjects.Add(internalName, allObjects.ToList());
        }

        static void PackSceneBundle(string bundleName, List<GUID> includedScenes, IDependencyInfo dependencyInfo, IBundleWriteInfo writeInfo, IDeterministicIdentifiers packingMethod, Dictionary<GUID, List<GUID>> assetToReferences)
        {
            var firstFileName = "";
            foreach (var scene in includedScenes)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(scene.ToString());
                var internalSceneName = packingMethod.GenerateInternalFileName(scenePath);
                if (string.IsNullOrEmpty(firstFileName))
                    firstFileName = internalSceneName;
                var internalName = string.Format(k_SceneBundleNameFormat, firstFileName, internalSceneName);

                SceneDependencyInfo sceneInfo = dependencyInfo.SceneInfo[scene];

                var references = new List<ObjectIdentifier>();
                references.AddRange(sceneInfo.referencedObjects);
                assetToReferences[scene] = FilterReferencesForAsset(dependencyInfo, scene, references);

                writeInfo.FileToObjects.Add(internalName, references);
                writeInfo.FileToBundle.Add(internalName, bundleName);
                writeInfo.AssetToFiles[scene] = new List<string> { internalName };
            }
        }

        static List<GUID> FilterReferencesForAsset(IDependencyInfo dependencyInfo, GUID asset, List<ObjectIdentifier> references)
        {
            var referencedAssets = new HashSet<AssetLoadInfo>();

            // First pass: Remove Default Resources and Includes for Assets assigned to Bundles
            for (int i = references.Count - 1; i >= 0; --i)
            {
                var reference = references[i];
                if (reference.filePath == k_UnityDefaultResourcePath)
                {
                    references.RemoveAt(i);
                    continue; // TODO: Fix this so we can pull these in
                }

                AssetLoadInfo referenceInfo;
                if (dependencyInfo.AssetInfo.TryGetValue(reference.guid, out referenceInfo))
                {
                    references.RemoveAt(i);
                    referencedAssets.Add(referenceInfo);
                    continue;
                }
            }

            // Second pass: Remove References also included by non-circular Referenced Assets
            foreach (var referencedAsset in referencedAssets)
            {
                var circularRef = referencedAsset.referencedObjects.Select(x => x.guid).Contains(asset);
                if (circularRef)
                    continue;

                references.RemoveAll(x => referencedAsset.referencedObjects.Contains(x));
            }

            // Final pass: Remove References also included by circular Referenced Assets if Asset's GUID is higher than Referenced Asset's GUID
            foreach (var referencedAsset in referencedAssets)
            {
                var circularRef = referencedAsset.referencedObjects.Select(x => x.guid).Contains(asset);
                if (!circularRef)
                    continue;

                if (asset < referencedAsset.asset)
                    continue;

                references.RemoveAll(x => referencedAsset.referencedObjects.Contains(x));
            }
            return referencedAssets.Select(x => x.asset).ToList();
        }
    }
}
