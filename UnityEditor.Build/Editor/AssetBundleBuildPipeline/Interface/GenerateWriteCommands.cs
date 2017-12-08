using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.AssetBundle.DataTypes;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    public class GenerateWriteCommands : IBuildTask<IDependencyInfo, IWriteInfo>, IBuildTask
    {
        public const string kUnityDefaultResourcePath = "library/unity default resources";

        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public IDeterministicIdentifiers PackingMethod { get; protected set; }

        public GenerateWriteCommands(IDeterministicIdentifiers packingMethod)
        {
            PackingMethod = packingMethod;
        }

        public BuildPipelineCodes Run(object context)
        {
            return Run((IDependencyInfo)context, (IWriteInfo)context);
        }

        public BuildPipelineCodes Run(IDependencyInfo input, IWriteInfo output)
        {
            foreach (var bundle in input.BundleToAssets)
            {
                // TODO: Handle Player Data & Raw write formats
                if (IsAssetBundle(bundle.Value))
                {
                    var op = CreateAssetBundleWriteOperation(bundle.Key, bundle.Value, input);
                    output.AssetBundles.Add(bundle.Key, op);
                }
                else if (IsSceneBundle(bundle.Value))
                {
                    var ops = CreateSceneBundleWriteOperations(bundle.Key, bundle.Value, input);
                    output.SceneBundles.Add(bundle.Key, ops);
                }
            }
            return BuildPipelineCodes.Success;
        }

        protected bool IsAssetBundle(List<GUID> assets)
        {
            foreach (var asset in assets)
            {
                if (ExtensionMethods.ValidAsset(asset))
                    continue;
                return false;
            }
            return true;
        }

        protected bool IsSceneBundle(List<GUID> assets)
        {
            foreach (var asset in assets)
            {
                if (ExtensionMethods.ValidScene(asset))
                    continue;
                return false;
            }
            return true;
        }

        protected IWriteOperation CreateAssetBundleWriteOperation(string bundleName, List<GUID> assets, IDependencyInfo input)
        {
            var dependencies = new HashSet<string>();
            var serializeObjects = new HashSet<ObjectIdentifier>();

            var op = new AssetBundleWriteOperation();
            op.command.fileName = PackingMethod.GenerateInternalFileName(bundleName);
            op.command.internalName = string.Format("archive:/{0}/{0}", op.command.fileName);

            op.info.bundleName = bundleName;
            op.info.bundleAssets = new List<AssetLoadInfo>();
            foreach (var asset in assets)
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
                foreach (var reference in assetInfo.referencedObjects)
                {
                    if (reference.filePath == kUnityDefaultResourcePath)
                        continue;

                    if (input.AssetInfo.ContainsKey(reference.guid))
                        continue;

                    serializeObjects.Add(reference);
                }
            }
            dependencies.Remove(bundleName); // Don't include self as dependency

            op.info.bundleDependencies = dependencies.OrderBy(x => x).ToList();
            op.command.dependencies = op.info.bundleDependencies.Select(x => string.Format("archive:/{0}/{0}", PackingMethod.GenerateInternalFileName(x))).ToList();
            op.command.serializeObjects = serializeObjects.Select(x => new SerializationInfo
            {
                serializationObject = x,
                serializationIndex = PackingMethod.SerializationIndexFromObjectIdentifier(x)
            }).ToList();

            return op;
        }

        protected List<IWriteOperation> CreateSceneBundleWriteOperations(string bundleName, List<GUID> scenes, IDependencyInfo input)
        {
            // The 'Folder' we mount asset bundles to is the same as the internal file name of the first file in the archive
            var bundleFileName = PackingMethod.GenerateInternalFileName(AssetDatabase.GUIDToAssetPath(scenes[0].ToString()));

            var ops = new List<SceneDataWriteOperation>();
            var sceneLoadInfo = new List<SceneLoadInfo>();
            var dependencies = new HashSet<string>();
            foreach (var scene in scenes)
            {
                var op = CreateSceneDataWriteOperation(bundleName, bundleFileName, scene, input);
                ops.Add((SceneDataWriteOperation)op);

                var scenePath = AssetDatabase.GUIDToAssetPath(scene.ToString());
                sceneLoadInfo.Add(new SceneLoadInfo
                {
                    asset = scene,
                    address = input.SceneAddress[scene],
                    internalName = PackingMethod.GenerateInternalFileName(scenePath)
                });

                dependencies.UnionWith(input.AssetToBundles[scene]);
            }
            dependencies.Remove(bundleName); // Don't include self as dependency

            // First write op must be SceneBundleWriteOperation
            var bundleOp = new SceneBundleWriteOperation(ops[0]);
            ops[0] = bundleOp;
            foreach (var serializeObj in bundleOp.command.serializeObjects)
                serializeObj.serializationIndex++; // Shift by 1 to account for asset bundle object

            bundleOp.info.bundleName = bundleName;
            bundleOp.info.bundleScenes = sceneLoadInfo;
            bundleOp.info.bundleDependencies = dependencies.OrderBy(x => x).ToList();

            return ops.Cast<IWriteOperation>().ToList();
        }

        protected IWriteOperation CreateSceneDataWriteOperation(string bundleName, string bundleFileName, GUID scene, IDependencyInfo input)
        {
            var sceneInfo = input.SceneInfo[scene];

            var op = new SceneDataWriteOperation();
            op.scene = sceneInfo.scene;
            op.processedScene = sceneInfo.processedScene;
            op.command.fileName = PackingMethod.GenerateInternalFileName(sceneInfo.scene) + ".sharedAssets";
            // TODO: This is bundle formatted internal name, we need to rethink this for PlayerData
            op.command.internalName = string.Format("archive:/{0}/{1}", bundleFileName, op.command.fileName);
            // TODO: Rethink the way we do dependencies here, assetToBundles is for bundles only, won't work for PlayerData or Raw Data.
            op.command.dependencies = input.AssetToBundles[scene].OrderBy(x => x).Where(x => x != bundleName).Select(x => string.Format("archive:/{0}/{0}", PackingMethod.GenerateInternalFileName(x))).ToList();
            input.SceneUsage.TryGetValue(scene, out op.usageTags);
            op.command.serializeObjects = new List<SerializationInfo>();
            op.preloadInfo.preloadObjects = new List<ObjectIdentifier>();
            op.preloadInfo.explicitDataLayout = true;
            long identifier = 2; // Scenes use linear id assignment
            foreach (var reference in sceneInfo.referencedObjects)
            {
                if (!input.AssetInfo.ContainsKey(reference.guid) && reference.filePath != kUnityDefaultResourcePath)
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
