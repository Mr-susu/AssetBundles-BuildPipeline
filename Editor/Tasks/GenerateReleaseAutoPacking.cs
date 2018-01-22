﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public struct GenerateReleaseAutoPacking : IBuildTask
    {
        // TODO: Move to utility file
        public const string k_UnityDefaultResourcePath = "library/unity default resources";

        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IDependencyInfo), typeof(IWriteInfo) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IDependencyInfo>(), context.GetContextObject<IWriteInfo>());
        }

        public static BuildPipelineCodes Run(IDependencyInfo dependencyInfo, IWriteInfo writeInfo)
        {
            // Object usage
            var objectUsage = new Dictionary<ObjectIdentifier, HashSet<string>>();

            // Calcualte the set of what is using every object
            foreach (var assetPair in dependencyInfo.AssetInfo)
            {
                AddObjectUsage(assetPair.Key.ToString(), assetPair.Value.includedObjects, objectUsage);
                AddObjectUsage(assetPair.Key.ToString(), assetPair.Value.referencedObjects, objectUsage);
            }

            foreach (var scenePair in dependencyInfo.SceneInfo)
            {
                AddObjectUsage(scenePair.Key.ToString(), scenePair.Value.referencedObjects, objectUsage);
            }

            // Calculate the optimal layout based on object usage
            foreach (var usagePair in objectUsage)
            {
                var usageList = usagePair.Value.ToList();
                Hash128 usageHash = HashingMethods.CalculateMD5Hash(usageList);

                List<ObjectIdentifier> objects;
                writeInfo.FileToObjects.GetOrAdd(usageHash.ToString(), out objects);
                objects.Add(usagePair.Key);
            }

            BuildLogger.LogWarning(writeInfo.FileToObjects.Count);
            LogContextToFile.Run(writeInfo.FileToObjects, @"D:\Projects\BuildHLAPI\Builds\ObjectLayout.json");

            foreach (var assetPair in dependencyInfo.AssetInfo)
            {
                List<string> hashes;
                writeInfo.AssetToFiles.GetOrAdd(assetPair.Key, out hashes);

                AddObjectDependencies(objectUsage, assetPair.Value.includedObjects, hashes);
                AddObjectDependencies(objectUsage, assetPair.Value.referencedObjects, hashes);
            }

            foreach (var scenePair in dependencyInfo.SceneInfo)
            {
                List<string> hashes;
                writeInfo.AssetToFiles.GetOrAdd(scenePair.Key, out hashes);
                hashes.Add(scenePair.Key.ToString());

                AddObjectDependencies(objectUsage, scenePair.Value.referencedObjects, hashes);
            }

            BuildLogger.LogWarning(writeInfo.AssetToFiles.Count);
            LogContextToFile.Run(writeInfo.AssetToFiles, @"D:\Projects\BuildHLAPI\Builds\AssetDependencies.json");

            return BuildPipelineCodes.Success;
        }

        static void AddObjectUsage(string source, IEnumerable<ObjectIdentifier> objectIDs, Dictionary<ObjectIdentifier, HashSet<string>> outObjectUsage)
        {
            foreach (ObjectIdentifier objectID in objectIDs)
            {
                if (objectID.filePath == k_UnityDefaultResourcePath)    // TODO: Fix this so we can pull in these objects
                    continue;

                HashSet<string> usage;
                outObjectUsage.GetOrAdd(objectID, out usage);
                usage.Add(source);
            }
        }

        static void AddObjectDependencies(Dictionary<ObjectIdentifier, HashSet<string>> objectUsage, IEnumerable<ObjectIdentifier> objectIDs, List<string> outDependencies)
        {
            foreach (var objectID in objectIDs)
            {
                if (objectID.filePath == k_UnityDefaultResourcePath)    // TODO: Fix this so we can pull in these objects
                    continue;

                var usageList = objectUsage[objectID].ToList();
                Hash128 usageHash = HashingMethods.CalculateMD5Hash(usageList);
                if (!outDependencies.Contains(usageHash.ToString()))
                    outDependencies.Add(usageHash.ToString());
            }
        }
    }
}
