using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.WriteTypes;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Tasks
{
    public struct GenerateWriteCommands : IBuildTask
    {
        // TODO: Move to utility file
        public const string k_UnityDefaultResourcePath = "library/unity default resources";

        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IBuildLayout), typeof(IDependencyInfo), typeof(IWriteInfo) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public IDeterministicIdentifiers PackingMethod { get; private set; }

        public GenerateWriteCommands(IDeterministicIdentifiers packingMethod) : this()
        {
            PackingMethod = packingMethod;
        }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(PackingMethod, context.GetContextObject<IBuildLayout>(), context.GetContextObject<IDependencyInfo>(), context.GetContextObject<IWriteInfo>());
        }

        public static BuildPipelineCodes Run(IDeterministicIdentifiers packingMethod, IBuildLayout buildLayout, IDependencyInfo dependencyInfo, IWriteInfo output)
        {
            foreach (KeyValuePair<string, List<GUID>> bundle in dependencyInfo.BundleToAssets)
            {
                // TODO: Handle Player Data & Raw write formats
                if (ExtensionMethods.ValidAssetBundle(bundle.Value))
                {
                    var op = CreateAssetBundleWriteOperation(packingMethod, bundle.Key, bundle.Value, dependencyInfo);
                    output.AssetBundles.Add(bundle.Key, op);
                }
                else if (ExtensionMethods.ValidSceneBundle(bundle.Value))
                {
                    var ops = CreateSceneBundleWriteOperations(packingMethod, bundle.Key, bundle.Value, buildLayout, dependencyInfo);
                    output.SceneBundles.Add(bundle.Key, ops);
                }
            }
            return BuildPipelineCodes.Success;
        }

        static IWriteOperation CreateAssetBundleWriteOperation(IDeterministicIdentifiers packingMethod, string bundleName, List<GUID> assets, IDependencyInfo input)
        {
            var dependencies = new HashSet<string>();
            var serializeObjects = new HashSet<ObjectIdentifier>();

            var op = new AssetBundleWriteOperation();
            op.command.fileName = packingMethod.GenerateInternalFileName(bundleName);
            op.command.internalName = string.Format("archive:/{0}/{0}", op.command.fileName);

            op.info.bundleName = bundleName;
            op.info.bundleAssets = new List<AssetLoadInfo>();
            foreach (GUID asset in assets)
            {
                AssetLoadInfo assetInfo;
                if (!input.AssetInfo.TryGetValue(asset, out assetInfo))
                {
                    BuildLogger.LogWarning("Could not find info for asset '{0}'.", asset);
                    continue;
                }

                op.info.bundleAssets.Add(assetInfo);

                dependencies.UnionWith(input.AssetToBundles[asset]);
                serializeObjects.UnionWith(assetInfo.includedObjects);
                foreach (ObjectIdentifier reference in assetInfo.referencedObjects)
                {
                    if (reference.filePath == k_UnityDefaultResourcePath)
                        continue;

                    if (input.AssetInfo.ContainsKey(reference.guid))
                        continue;

                    serializeObjects.Add(reference);
                }
            }
            dependencies.Remove(bundleName); // Don't include self as dependency

            op.info.bundleDependencies = dependencies.OrderBy(x => x).ToList();
            op.command.dependencies = op.info.bundleDependencies.Select(x => string.Format("archive:/{0}/{0}", packingMethod.GenerateInternalFileName(x))).ToList();
            op.command.serializeObjects = serializeObjects.Select(x => new SerializationInfo
            {
                serializationObject = x,
                serializationIndex = packingMethod.SerializationIndexFromObjectIdentifier(x)
            }).ToList();

            return op;
        }

        static List<IWriteOperation> CreateSceneBundleWriteOperations(IDeterministicIdentifiers packingMethod, string bundleName, List<GUID> scenes, IBuildLayout buildLayout, IDependencyInfo input)
        {
            // The 'Folder' we mount asset bundles to is the same as the internal file name of the first file in the archive
            string bundleFileName = packingMethod.GenerateInternalFileName(AssetDatabase.GUIDToAssetPath(scenes[0].ToString()));

            var ops = new List<IWriteOperation>();
            var sceneLoadInfo = new List<SceneLoadInfo>();
            var dependencies = new HashSet<string>();
            foreach (GUID scene in scenes)
            {
                IWriteOperation op = CreateSceneDataWriteOperation(packingMethod, bundleName, bundleFileName, scene, input);
                ops.Add(op);

                string scenePath = AssetDatabase.GUIDToAssetPath(scene.ToString());
                sceneLoadInfo.Add(new SceneLoadInfo
                {
                    asset = scene,
                    address = buildLayout.Addresses[scene],
                    internalName = packingMethod.GenerateInternalFileName(scenePath)
                });

                dependencies.UnionWith(input.AssetToBundles[scene]);
            }
            dependencies.Remove(bundleName); // Don't include self as dependency

            // First write op must be SceneBundleWriteOperation
            var bundleOp = new SceneBundleWriteOperation((SceneDataWriteOperation)ops[0]);
            ops[0] = bundleOp;
            foreach (SerializationInfo serializeObj in bundleOp.command.serializeObjects)
                serializeObj.serializationIndex++; // Shift by 1 to account for asset bundle object

            bundleOp.info.bundleName = bundleName;
            bundleOp.info.bundleScenes = sceneLoadInfo;
            bundleOp.info.bundleDependencies = dependencies.OrderBy(x => x).ToList();

            return ops;
        }

        static IWriteOperation CreateSceneDataWriteOperation(IDeterministicIdentifiers packingMethod, string bundleName, string bundleFileName, GUID scene, IDependencyInfo input)
        {
            SceneDependencyInfo sceneInfo = input.SceneInfo[scene];

            var op = new SceneDataWriteOperation();
            op.scene = sceneInfo.scene;
            op.processedScene = sceneInfo.processedScene;
            op.command.fileName = packingMethod.GenerateInternalFileName(sceneInfo.scene) + ".sharedAssets";
            // TODO: This is bundle formatted internal name, we need to rethink this for PlayerData
            op.command.internalName = string.Format("archive:/{0}/{1}", bundleFileName, op.command.fileName);
            // TODO: Rethink the way we do dependencies here, AssetToBundles is for bundles only, won't work for PlayerData or Raw Data.
            op.command.dependencies = input.AssetToBundles[scene].OrderBy(x => x).Where(x => x != bundleName).Select(x => string.Format("archive:/{0}/{0}", packingMethod.GenerateInternalFileName(x))).ToList();
            input.SceneUsage.TryGetValue(scene, out op.usageTags);
            op.command.serializeObjects = new List<SerializationInfo>();
            op.preloadInfo.preloadObjects = new List<ObjectIdentifier>();
            op.preloadInfo.explicitDataLayout = true;
            long identifier = 2; // Scenes use linear id assignment
            foreach (ObjectIdentifier reference in sceneInfo.referencedObjects)
            {
                if (!input.AssetInfo.ContainsKey(reference.guid) && reference.filePath != k_UnityDefaultResourcePath)
                {
                    op.command.serializeObjects.Add(new SerializationInfo
                    {
                        serializationObject = reference,
                        serializationIndex = identifier++
                    });
                }
                else
                    op.preloadInfo.preloadObjects.Add(reference);
            }

            // TODO: Add this functionality:
            // Unique to scenes, we point at sharedAssets of a previously built scene in this set as a dependency to reduce object duplication. 

            return op;
        }
    }
}
