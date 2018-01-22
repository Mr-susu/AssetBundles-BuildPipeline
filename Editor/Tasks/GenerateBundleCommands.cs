using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.UnityPackages.BuildPipeline.Editor.Shared;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Build.WriteTypes;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Tasks
{
    public struct GenerateBundleCommands : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IBundleLayout), typeof(IBuildContent), typeof(IDependencyInfo), typeof(IBundleWriteInfo), typeof(IDeterministicIdentifiers) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBundleLayout>(), context.GetContextObject<IBuildContent>(), context.GetContextObject<IDependencyInfo>(),
                context.GetContextObject<IBundleWriteInfo>(), context.GetContextObject<IDeterministicIdentifiers>());
        }

        public static BuildPipelineCodes Run(IBundleLayout bundleLayout, IBuildContent buildContent, IDependencyInfo dependencyInfo, IBundleWriteInfo writeInfo, IDeterministicIdentifiers packingMethod)
        {
            foreach (var bundlePair in bundleLayout.ExplicitLayout)
            {
                if (ExtensionMethods.ValidAssetBundle(bundlePair.Value))
                {
                    CreateAssetBundleCommand(bundlePair.Key, writeInfo.AssetToFiles[bundlePair.Value[0]][0], bundlePair.Value, dependencyInfo, writeInfo, packingMethod);
                }
                else if (ExtensionMethods.ValidSceneBundle(bundlePair.Value))
                {
                    CreateSceneBundleCommand(bundlePair.Key, writeInfo.AssetToFiles[bundlePair.Value[0]][0], bundlePair.Value[0], bundlePair.Value, buildContent, dependencyInfo, writeInfo);
                    for (int i = 1; i < bundlePair.Value.Count; ++i)
                        CreateSceneDataCommand(writeInfo.AssetToFiles[bundlePair.Value[i]][0], bundlePair.Value[i], dependencyInfo, writeInfo);
                }
            }
            return BuildPipelineCodes.Success;
        }

        static WriteCommand CreateWriteCommand(string internalName, List<ObjectIdentifier> objects, IDeterministicIdentifiers packingMethod)
        {
            var command = new WriteCommand();
            command.internalName = internalName;
            command.fileName = Path.GetFileName(internalName); // TODO: Maybe remove this from C++?
            // command.dependencies // TODO: Definitely remove this from C++

            command.serializeObjects = objects.Select(x => new SerializationInfo
            {
                serializationObject = x,
                serializationIndex = packingMethod.SerializationIndexFromObjectIdentifier(x)
            }).ToList();
            return command;
        }

        static void CreateAssetBundleCommand(string bundleName, string internalName, List<GUID> assets, IDependencyInfo dependencyInfo, IBundleWriteInfo writeInfo, IDeterministicIdentifiers packingMethod)
        {
            var abOp = new AssetBundleWriteOperation();
            abOp.usageSet = writeInfo.FileToUsageSet[internalName];
            abOp.referenceMap = writeInfo.FileToReferenceMap[internalName];

            var fileObjects = writeInfo.FileToObjects[internalName];
            abOp.command = CreateWriteCommand(internalName, fileObjects, packingMethod);

            {
                abOp.info = new AssetBundleInfo();
                abOp.info.bundleName = bundleName;

                var dependencies = new HashSet<string>();
                var bundles = assets.SelectMany(x => writeInfo.AssetToFiles[x].Select(y => writeInfo.FileToBundle[y]));
                dependencies.UnionWith(bundles);
                dependencies.Remove(bundleName);

                abOp.info.bundleDependencies = dependencies.OrderBy(x => x).ToList();
                abOp.info.bundleAssets = assets.Select(x => dependencyInfo.AssetInfo[x]).ToList();
            }

            writeInfo.WriteOperations.Add(abOp);
        }

        static void CreateSceneBundleCommand(string bundleName, string internalName, GUID asset, List<GUID> assets, IBuildContent buildContent, IDependencyInfo dependencyInfo, IBundleWriteInfo writeInfo)
        {
            var sbOp = new SceneBundleWriteOperation();
            sbOp.usageSet = writeInfo.FileToUsageSet[internalName];
            sbOp.referenceMap = writeInfo.FileToReferenceMap[internalName];

            var fileObjects = writeInfo.FileToObjects[internalName];
            sbOp.command = CreateWriteCommand(internalName, fileObjects, new LinearPackedIdentifiers(3)); // Start at 3: PreloadData = 1, AssetBundle = 2

            var sceneInfo = dependencyInfo.SceneInfo[asset];
            sbOp.scene = sceneInfo.scene;
            sbOp.processedScene = sceneInfo.processedScene;

            {
                sbOp.preloadInfo = new PreloadInfo();
                sbOp.preloadInfo.preloadObjects = sceneInfo.referencedObjects.Where(x => !fileObjects.Contains(x)).ToList();
            }

            {
                sbOp.info = new SceneBundleInfo();
                sbOp.info.bundleName = bundleName;

                var dependencies = new HashSet<string>();
                var bundles = assets.SelectMany(x => writeInfo.AssetToFiles[x].Select(y => writeInfo.FileToBundle[y]));
                dependencies.UnionWith(bundles);
                dependencies.Remove(bundleName);

                sbOp.info.bundleDependencies = dependencies.OrderBy(x => x).ToList();
                sbOp.info.bundleScenes = assets.Select(x =>
                {
                    return new SceneLoadInfo
                    {
                        asset = x,
                        internalName = writeInfo.AssetToFiles[x][0],
                        address = buildContent.Addresses[x]
                    };
                }).ToList();
            }
        }

        static void CreateSceneDataCommand(string internalName, GUID asset, IDependencyInfo dependencyInfo, IBundleWriteInfo writeInfo)
        {
            var sbOp = new SceneDataWriteOperation();
            sbOp.usageSet = writeInfo.FileToUsageSet[internalName];
            sbOp.referenceMap = writeInfo.FileToReferenceMap[internalName];

            var fileObjects = writeInfo.FileToObjects[internalName];
            sbOp.command = CreateWriteCommand(internalName, fileObjects, new LinearPackedIdentifiers(2)); // Start at 2: PreloadData = 1

            var sceneInfo = dependencyInfo.SceneInfo[asset];
            sbOp.scene = sceneInfo.scene;
            sbOp.processedScene = sceneInfo.processedScene;

            {
                sbOp.preloadInfo = new PreloadInfo();
                sbOp.preloadInfo.preloadObjects = sceneInfo.referencedObjects.Where(x => !fileObjects.Contains(x)).ToList();
            }
        }
    }
}
