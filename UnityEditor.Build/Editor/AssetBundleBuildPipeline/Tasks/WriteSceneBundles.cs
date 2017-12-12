﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.WriteTypes;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public class WriteSceneBundles : WriteFileBase
    {
        protected override Hash128 CalculateInputHash(IBuildParams buildParams, IWriteOperation operation, List<WriteCommand> dependencies, BuildUsageTagGlobal globalUsage, BuildUsageTagSet buildUsage)
        {
            if (!buildParams.UseCache)
                return new Hash128();

            var hashes = new List<Hash128>();
            var visitedAssets = new HashSet<GUID>();
            IEnumerable<ObjectIdentifier> objectIDs = operation.command.serializeObjects.Select(x => x.serializationObject);
            foreach (ObjectIdentifier objectID in objectIDs)
            {
                if (objectID.fileType != FileType.MetaAssetType && objectID.fileType != FileType.SerializedAssetType)
                    continue;

                if (!visitedAssets.Add(objectID.guid))
                    continue;

                string path = AssetDatabase.GUIDToAssetPath(objectID.guid.ToString());
                hashes.Add(AssetDatabase.GetAssetDependencyHash(path));
            }

            var op = operation as SceneDataWriteOperation;
            if (op != null)
                hashes.Add(HashingMethods.CalculateFileMD5Hash(op.processedScene));

            return HashingMethods.CalculateMD5Hash(Version, operation, hashes, dependencies, globalUsage, buildUsage, buildParams.Settings);
        }

        public override BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo input1, IWriteInfo input2, IResultInfo output)
        {
            List<WriteCommand> allCommands = new List<WriteCommand>(input2.AssetBundles.Select(x => x.Value.command));
            allCommands.AddRange(input2.SceneBundles.SelectMany(x => x.Value.Select(y => y.command)));

            foreach (KeyValuePair<string, List<IWriteOperation>> bundle in input2.SceneBundles)
            {
                foreach (IWriteOperation op in bundle.Value)
                    WriteSerialziedFiles(buildParams, bundle.Key, op, allCommands, input1.GlobalUsage, output);
            }

            return BuildPipelineCodes.Success;
        }
    }
}
