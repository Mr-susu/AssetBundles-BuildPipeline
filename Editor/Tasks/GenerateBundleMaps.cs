using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Interfaces;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public struct GenerateBundleMaps : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IDependencyInfo), typeof(IBundleWriteInfo) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IDependencyInfo>(), context.GetContextObject<IBundleWriteInfo>());
        }

        public static BuildPipelineCodes Run(IDependencyInfo dependencyInfo, IBundleWriteInfo writeInfo)
        {
            foreach (var assetFilesPair in writeInfo.AssetToFiles)
            {
                // Generate BuildReferenceMap map
                AddReferencesForFiles(assetFilesPair.Value, writeInfo);

                // Generate BuildUsageTagSet map
                AddUsageSetForFiles(assetFilesPair.Key, assetFilesPair.Value, dependencyInfo, writeInfo);
            }

            return BuildPipelineCodes.Success;
        }

        static void AddReferencesForFiles(IList<string> files, IBundleWriteInfo writeInfo)
        {
            BuildReferenceMap referenceMap;
            if (!writeInfo.FileToReferenceMap.TryGetValue(files[0], out referenceMap))
            {
                referenceMap = new BuildReferenceMap();
                writeInfo.FileToReferenceMap.Add(files[0], referenceMap);
            }

            var fileToCommand = writeInfo.WriteOperations.ToDictionary(x => x.command.internalName, x => x.command);
            foreach (var file in files)
            {
                var command = fileToCommand[file];
                referenceMap.AddMappings(file, command.serializeObjects.ToArray());
            }
        }

        static void AddUsageSetForFiles(GUID asset, IList<string> files, IDependencyInfo dependencyInfo, IBundleWriteInfo writeInfo)
        {
            BuildUsageTagSet assetUsage;
            if (!dependencyInfo.AssetUsage.TryGetValue(asset, out assetUsage))
            {
                if (!dependencyInfo.SceneUsage.TryGetValue(asset, out assetUsage))
                    return;
            }

            foreach (var file in files)
            {
                BuildUsageTagSet fileUsage;
                if (!writeInfo.FileToUsageSet.TryGetValue(file, out fileUsage))
                {
                    fileUsage = new BuildUsageTagSet();
                    writeInfo.FileToUsageSet.Add(file, fileUsage);
                }
                fileUsage.UnionWith(assetUsage);
            }
        }
    }
}
