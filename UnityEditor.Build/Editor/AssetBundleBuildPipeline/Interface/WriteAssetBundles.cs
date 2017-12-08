using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.AssetBundle.DataTypes;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public class WriteAssetBundles : IBuildTask<IDependencyInfo, IWriteInfo, IResultInfo>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        private Hash128 CalculateInputHash(IWriteOperation operation, List<WriteCommand> dependencies, BuildUsageTagGlobal globalUsage, BuildUsageTagSet buildUsage)
        {
            if (!BuildParams.UseCache)
                return new Hash128();
            
            var hashes = new List<Hash128>();
            var visitedAssets = new HashSet<GUID>();
            var objectIDs = operation.command.serializeObjects.Select(x => x.serializationObject);
            foreach (var objectID in objectIDs)
            {
                if (objectID.fileType != FileType.MetaAssetType && objectID.fileType != FileType.SerializedAssetType)
                    continue;
                
                if (!visitedAssets.Add(objectID.guid))
                    continue;

                var path = AssetDatabase.GUIDToAssetPath(objectID.guid.ToString());
                hashes.Add(AssetDatabase.GetAssetDependencyHash(path));
            }

            return HashingMethods.CalculateMD5Hash(Version, operation, hashes, dependencies, globalUsage, buildUsage, BuildParams.Settings);
        }

        public BuildPipelineCodes Run(object context)
        {
            return Run((IDependencyInfo)context, (IWriteInfo)context, (IResultInfo)context);
        }

        public BuildPipelineCodes Run(IDependencyInfo input1, IWriteInfo input2, IResultInfo output)
        {
            var allCommands = new List<WriteCommand>(input2.AssetBundles.Select(x => x.Value.command));
            allCommands.AddRange(input2.SceneBundles.SelectMany(x => x.Value.Select(y => y.command)));

            foreach (var bundle in input2.AssetBundles)
                WriteSerialziedFiles(bundle.Key, bundle.Value, allCommands, input1.GlobalUsage, output);

            return BuildPipelineCodes.Success;
        }

        protected void WriteSerialziedFiles(string bundleName, IWriteOperation op, List<WriteCommand> allCommands, BuildUsageTagGlobal globalUsage, IResultInfo output)
        {
            var dependencies = op.CalculateDependencies(allCommands);
            
            var objectIDs = op.command.serializeObjects.Select(x => x.serializationObject).ToArray();
            var dependentIDs = dependencies.SelectMany(x => x.serializeObjects.Select(y => y.serializationObject)).ToArray();
            
            BuildUsageTagSet buildUsage = new BuildUsageTagSet();
            BundleBuildInterface.CalculateBuildUsageTags(objectIDs, dependentIDs, globalUsage, buildUsage);
            
            WriteResult result;

            Hash128 hash = CalculateInputHash(op, dependencies, globalUsage, buildUsage);
            if (BuildParams.UseCache && BuildCache.TryLoadCachedResults(hash, out result))
            {
                SetOutputInformation(bundleName, result, output);
                return;
            }

            result = op.Write(BuildParams.GetTempOrCacheBuildPath(hash), dependencies, BuildParams.Settings, globalUsage, buildUsage);
            SetOutputInformation(bundleName, result, output);
            
            if (BuildParams.UseCache && !BuildCache.SaveCachedResults(hash, result))
                BuildLogger.LogWarning("Unable to cache CommandSetWriter results for command '{0}'.", op.command.internalName);
        }

        protected void SetOutputInformation(string bundleName, WriteResult result, IResultInfo output)
        {
            List<WriteResult> results;
            output.BundleResults.GetOrAdd(bundleName, out results);
            results.Add(result);
        }
    }
}
