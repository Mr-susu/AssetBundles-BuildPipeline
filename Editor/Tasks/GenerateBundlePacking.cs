using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Tasks
{
    public struct GenerateBundlePacking : IBuildTask
    {
        // TODO: Move to utility file
        public const string k_UnityDefaultResourcePath = "library/unity default resources";

        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IBuildLayout), typeof(IDependencyInfo), typeof(IPackingInfo) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildLayout>(), context.GetContextObject<IDependencyInfo>(), context.GetContextObject<IPackingInfo>());
        }

        public static BuildPipelineCodes Run(IBuildLayout buildLayout, IDependencyInfo dependencyInfo, IPackingInfo packingInfo)
        {
            foreach (KeyValuePair<string, List<GUID>> bundle in buildLayout.ExplicitLayout)
            {
                foreach (GUID asset in bundle.Value)
                {
                    // Add the current bundle as dependency[0]
                    var bundles = new List<string> { bundle.Key };
                    packingInfo.AssetToFiles.Add(asset, bundles);
                }
            }

            // Layout Asset Bundle Dependencies
            foreach (var asset in dependencyInfo.AssetInfo)
            {
                List<string> files = packingInfo.AssetToFiles[asset.Key];
                AddFilesForReferences(packingInfo, files, asset.Value.referencedObjects);

                List<ObjectIdentifier> objectIDs;
                packingInfo.FileToObjects.GetOrAdd(files[0], out objectIDs);

                AddObjectsToFile(dependencyInfo, objectIDs, asset.Value.includedObjects, false);
                AddObjectsToFile(dependencyInfo, objectIDs, asset.Value.referencedObjects, true);
            }

            // Layout Scene Bundle Dependencies
            foreach (var scene in dependencyInfo.SceneInfo)
            {
                List<string> files = packingInfo.AssetToFiles[scene.Key];
                AddFilesForReferences(packingInfo, files, scene.Value.referencedObjects);

                // Notes: Each scene is a serialzied file, so store the file to objects based on scene guid instead of asset bundle name
                List<ObjectIdentifier> objectIDs;
                packingInfo.FileToObjects.GetOrAdd(scene.Key.ToString(), out objectIDs);

                AddObjectsToFile(dependencyInfo, objectIDs, scene.Value.referencedObjects, true);
            }

            return BuildPipelineCodes.Success;
        }

        static void AddFilesForReferences(IPackingInfo packingInfo, ICollection<string> files, IEnumerable<ObjectIdentifier> references)
        {
            foreach (var reference in references)
            {
                List<string> referenceFiles;
                if (!packingInfo.AssetToFiles.TryGetValue(reference.guid, out referenceFiles))
                    continue;

                var dependency = referenceFiles[0];
                if (files.Contains(dependency))
                    continue;

                files.Add(dependency);
            }
        }

        static void AddObjectsToFile(IDependencyInfo dependencyInfo, ICollection<ObjectIdentifier> fileObjects, IEnumerable<ObjectIdentifier> objectIDs, bool references)
        {
            foreach (ObjectIdentifier objectID in objectIDs)
            {
                if (objectID.filePath == k_UnityDefaultResourcePath)
                    continue;

                if (references && dependencyInfo.AssetInfo.ContainsKey(objectID.guid))
                    continue;

                if (fileObjects.Contains(objectID))
                    continue;

                fileObjects.Add(objectID);
            }
        }
    }
}
