using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public struct CalcualteObjectDependencyHash : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IDependencyInfo) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IDependencyInfo>());
        }

        public static BuildPipelineCodes Run(IDependencyInfo dependencyInfo)
        {
            var objectUsage = new Dictionary<ObjectIdentifier, HashSet<string>>();

            // Calcualte the set of what is using every object
            foreach (var assetPair in dependencyInfo.AssetInfo)
            {
                var assetInfo = assetPair.Value;
                string assetSource = assetPair.Key.ToString();

                // Use explicit assignment if one was passed in
                List<string> bundles;
                if (dependencyInfo.AssetToBundles.TryGetValue(assetPair.Key, out bundles))
                    assetSource = bundles.First();

                foreach (var include in assetInfo.includedObjects)
                {
                    HashSet<string> usage;
                    objectUsage.GetOrAdd(include, out usage);
                    usage.Add(assetSource);
                }


                foreach (var reference in assetInfo.referencedObjects)
                {
                    HashSet<string> usage;
                    objectUsage.GetOrAdd(reference, out usage);
                    usage.Add(assetSource);
                }
            }

            foreach (var scenePair in dependencyInfo.SceneInfo)
            {
                var sceneInfo = scenePair.Value;
                string sceneSource = scenePair.Key.ToString();

                // Use explicit assignment if one was passed in
                List<string> bundles;
                if (dependencyInfo.AssetToBundles.TryGetValue(scenePair.Key, out bundles))
                    sceneSource = bundles.First();

                foreach (var reference in sceneInfo.referencedObjects)
                {
                    HashSet<string> usage;
                    objectUsage.GetOrAdd(reference, out usage);
                    usage.Add(sceneSource);
                }
            }

            // File Layout
            var hashToObjectIDs = new Dictionary<string, List<ObjectIdentifier>>();

            // Calculate the optimal layout based on usage
            foreach (var usagePair in objectUsage)
            {
                Hash128 usageHash = HashingMethods.CalculateMD5Hash(usagePair.Value.ToArray());

                List<ObjectIdentifier> objects;
                hashToObjectIDs.GetOrAdd(usageHash.ToString(), out objects);
                objects.Add(usagePair.Key);
            }

            BuildLogger.LogWarning(hashToObjectIDs.Count);
            LogContextToFile.Run(hashToObjectIDs, @"D:\Projects\BuildHLAPI\Builds\ObjectLayout.json");

            // File Dependencies
            var guidToHashes = new Dictionary<string, List<string>>();

            foreach (var assetPair in dependencyInfo.AssetInfo)
            {
                List<string> hashes;
                guidToHashes.GetOrAdd(assetPair.Key.ToString(), out hashes);

                var assetInfo = assetPair.Value;
                foreach (var include in assetInfo.includedObjects)
                {
                    Hash128 usageHash = HashingMethods.CalculateMD5Hash(objectUsage[include].ToArray());
                    if (!hashes.Contains(usageHash.ToString()))
                        hashes.Add(usageHash.ToString());
                }

                foreach (var reference in assetInfo.referencedObjects)
                {
                    Hash128 usageHash = HashingMethods.CalculateMD5Hash(objectUsage[reference].ToArray());
                    if (!hashes.Contains(usageHash.ToString()))
                        hashes.Add(usageHash.ToString());
                }
            }

            foreach (var scenePair in dependencyInfo.SceneInfo)
            {
                List<string> hashes;
                guidToHashes.GetOrAdd(scenePair.Key.ToString(), out hashes);
                hashes.Add(scenePair.Key.ToString());

                var sceneInfo = scenePair.Value;
                foreach (var reference in sceneInfo.referencedObjects)
                {
                    Hash128 usageHash = HashingMethods.CalculateMD5Hash(objectUsage[reference].ToArray());
                    if (!hashes.Contains(usageHash.ToString()))
                        hashes.Add(usageHash.ToString());
                }
            }

            BuildLogger.LogWarning(guidToHashes.Count);
            LogContextToFile.Run(guidToHashes, @"D:\Projects\BuildHLAPI\Builds\AssetDependencies.json");

            return BuildPipelineCodes.Success;
        }
    }
}
