using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.WriteTypes;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Tasks
{
    public struct GenerateBundleCommands : IBuildTask
    {
        // TODO: Move to utility file
        public const string k_UnityDefaultResourcePath = "library/unity default resources";

        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IDeterministicIdentifiers), typeof(IBuildLayout), typeof(IBuildContent), typeof(IDependencyInfo), typeof(IPackingInfo), typeof(IWriteInfo) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IDeterministicIdentifiers>(), context.GetContextObject<IBuildLayout>(), context.GetContextObject<IBuildContent>(),
                context.GetContextObject<IDependencyInfo>(), context.GetContextObject<IPackingInfo>(), context.GetContextObject<IWriteInfo>());
        }

        public static BuildPipelineCodes Run(IDeterministicIdentifiers packingMethod, IBuildLayout buildLayout, IBuildContent buildContent, IDependencyInfo dependencyInfo, IPackingInfo packingInfo, IWriteInfo output)
        {
            if (buildLayout.ExplicitLayout.IsNullOrEmpty())
            {
                BuildLogger.LogError("Running build pipeline that requires explicit bundle assignments without IBuildContent.ExplicitLayout populated");
                return BuildPipelineCodes.Error;
            }

            foreach (KeyValuePair<string, List<GUID>> bundle in buildLayout.ExplicitLayout)
            {
                if (ExtensionMethods.ValidAssetBundle(bundle.Value))
                {
                    var op = CreateAssetBundleWriteOperation(packingMethod, bundle.Key, bundle.Value, dependencyInfo, packingInfo);
                    output.AssetBundles.Add(bundle.Key, op);
                }
                else if (ExtensionMethods.ValidSceneBundle(bundle.Value))
                {
                    var ops = CreateSceneBundleWriteOperations(packingMethod, bundle.Key, bundle.Value, buildContent, dependencyInfo, packingInfo);
                    output.SceneBundles.Add(bundle.Key, ops);
                }
            }
            return BuildPipelineCodes.Success;
        }

        static IWriteOperation CreateAssetBundleWriteOperation(IDeterministicIdentifiers packingMethod, string bundleName, List<GUID> assets, IDependencyInfo dependencyInfo, IPackingInfo packingInfo)
        {
            var dependencies = new HashSet<string>();

            var op = new AssetBundleWriteOperation();

            op.info.bundleName = bundleName;
            op.info.bundleAssets = new List<AssetLoadInfo>();
            foreach (GUID asset in assets)
            {
                op.info.bundleAssets.Add(dependencyInfo.AssetInfo[asset]);
                dependencies.UnionWith(packingInfo.AssetToFiles[asset]);
            }
            dependencies.Remove(bundleName); // Don't include self as dependency
            op.info.bundleDependencies = dependencies.OrderBy(x => x).ToList();

            op.command.fileName = packingMethod.GenerateInternalFileName(bundleName);
            op.command.internalName = string.Format("archive:/{0}/{0}", op.command.fileName);
            op.command.dependencies = op.info.bundleDependencies.Select(x => string.Format("archive:/{0}/{0}", packingMethod.GenerateInternalFileName(x))).ToList();
            op.command.serializeObjects = packingInfo.FileToObjects[bundleName].Select(x => new SerializationInfo
            {
                serializationObject = x,
                serializationIndex = packingMethod.SerializationIndexFromObjectIdentifier(x)
            }).ToList();

            return op;
        }

        static List<IWriteOperation> CreateSceneBundleWriteOperations(IDeterministicIdentifiers packingMethod, string bundleName, List<GUID> scenes, IBuildContent buildContent, IDependencyInfo dependencyInfo, IPackingInfo packingInfo)
        {
            // The 'Folder' we mount for asset bundles is the internal file name of the first serialized file in the archive
            string bundleFileName = packingMethod.GenerateInternalFileName(AssetDatabase.GUIDToAssetPath(scenes[0].ToString()));

            var ops = new List<IWriteOperation>();
            var sceneLoadInfo = new List<SceneLoadInfo>();
            var dependencies = new HashSet<string>();
            foreach (GUID scene in scenes)
            {
                IWriteOperation op = CreateSceneDataWriteOperation(packingMethod, bundleName, bundleFileName, scene, dependencyInfo, packingInfo);
                ops.Add(op);

                string scenePath = AssetDatabase.GUIDToAssetPath(scene.ToString());
                sceneLoadInfo.Add(new SceneLoadInfo
                {
                    asset = scene,
                    address = buildContent.Addresses[scene],
                    internalName = packingMethod.GenerateInternalFileName(scenePath)
                });

                dependencies.UnionWith(packingInfo.AssetToFiles[scene]);
            }
            dependencies.Remove(bundleName); // Don't include self as dependency

            // First write op must be SceneBundleWriteOperation
            var bundleOp = new SceneBundleWriteOperation((SceneDataWriteOperation)ops[0]);
            foreach (SerializationInfo serializeObj in bundleOp.command.serializeObjects)
                serializeObj.serializationIndex++; // Shift by 1 to account for asset bundle object
            ops[0] = bundleOp;

            bundleOp.info.bundleName = bundleName;
            bundleOp.info.bundleScenes = sceneLoadInfo;
            bundleOp.info.bundleDependencies = dependencies.OrderBy(x => x).ToList();

            return ops;
        }

        static IWriteOperation CreateSceneDataWriteOperation(IDeterministicIdentifiers packingMethod, string bundleName, string bundleFileName, GUID scene, IDependencyInfo dependencyInfo, IPackingInfo packingInfo)
        {
            SceneDependencyInfo sceneInfo = dependencyInfo.SceneInfo[scene];
            var dependencies = packingInfo.AssetToFiles[scene].OrderBy(x => x).ToList();
            dependencies.Remove(bundleName); // Don't include self as dependency

            var op = new SceneDataWriteOperation();

            op.scene = sceneInfo.scene;
            op.processedScene = sceneInfo.processedScene;
            op.usageTags = dependencyInfo.SceneUsage[scene];

            op.command.fileName = packingMethod.GenerateInternalFileName(sceneInfo.scene) + ".sharedAssets";
            op.command.internalName = string.Format("archive:/{0}/{1}", bundleFileName, op.command.fileName);
            op.command.dependencies = dependencies.Select(x => string.Format("archive:/{0}/{0}", packingMethod.GenerateInternalFileName(x))).ToList();
            long identifier = 2; // Scenes use linear id assignment, starting at 2 (1 is preload object)
            op.command.serializeObjects = packingInfo.FileToObjects[scene.ToString()].Select(x => new SerializationInfo
            {
                serializationObject = x,
                serializationIndex = identifier++
            }).ToList();

            op.preloadInfo.preloadObjects = new List<ObjectIdentifier>();
            foreach (ObjectIdentifier reference in sceneInfo.referencedObjects)
            {
                if (dependencyInfo.AssetInfo.ContainsKey(reference.guid) || reference.filePath == k_UnityDefaultResourcePath)
                    op.preloadInfo.preloadObjects.Add(reference);
            }

            return op;
        }
    }
}
