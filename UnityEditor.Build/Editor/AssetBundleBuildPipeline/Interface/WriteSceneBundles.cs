using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.AssetBundle.DataTypes;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public class WriteSceneBundles : WriteFileBase
    {
        protected override Hash128 CalculateInputHash(IWriteOperation operation, List<WriteCommand> dependencies, BuildUsageTagGlobal globalUsage, BuildUsageTagSet buildUsage)
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

            var op = operation as SceneDataWriteOperation;
            if (op != null)
                hashes.Add(HashingMethods.CalculateFileMD5Hash(op.processedScene));

            return HashingMethods.CalculateMD5Hash(Version, operation, hashes, dependencies, globalUsage, buildUsage, BuildParams.Settings);
        }

        public override BuildPipelineCodes Run(IDependencyInfo input1, IWriteInfo input2, IResultInfo output)
        {
            var allCommands = new List<WriteCommand>(input2.AssetBundles.Select(x => x.Value.command));
            allCommands.AddRange(input2.SceneBundles.SelectMany(x => x.Value.Select(y => y.command)));

            foreach (var bundle in input2.SceneBundles)
            {
                foreach (var op in bundle.Value)
                    WriteSerialziedFiles(bundle.Key, op, allCommands, input1.GlobalUsage, output);
            }

            return BuildPipelineCodes.Success;
        }
    }
}
