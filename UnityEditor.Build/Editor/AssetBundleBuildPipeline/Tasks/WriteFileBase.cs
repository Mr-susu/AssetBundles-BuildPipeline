using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.WriteTypes;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public abstract class WriteFileBase : IBuildTask
    {
        public abstract int Version { get; }

        public abstract Type[] RequiredContextTypes { get; }

        public abstract BuildPipelineCodes Run(IBuildContext context);

        protected delegate Hash128 CalculateInputHashFunc(IBuildParams buildParams, IWriteOperation operation, List<WriteCommand> dependencies, BuildUsageTagGlobal globalUsage, BuildUsageTagSet buildUsage);

        protected static BuildPipelineCodes WriteSerialziedFiles(IBuildParams buildParams, string bundleName, IWriteOperation op, List<WriteCommand> allCommands, BuildUsageTagGlobal globalUsage,
            IResultInfo output, CalculateInputHashFunc calculateInputHash, IProgressTracker tracker = null)
        {
            List<WriteCommand> dependents = op.CalculateReverseDependencies(allCommands);

            ObjectIdentifier[] objectIDs = op.command.serializeObjects.Select(x => x.serializationObject).ToArray();
            ObjectIdentifier[] dependentIDs = dependents.SelectMany(x => x.serializeObjects.Select(y => y.serializationObject)).ToArray();

            var buildUsage = new BuildUsageTagSet();
            BundleBuildInterface.CalculateBuildUsageTags(objectIDs, dependentIDs, globalUsage, buildUsage);

            var result = new WriteResult();

            List<WriteCommand> dependencies = op.CalculateForwardDependencies(allCommands);

            Hash128 hash = calculateInputHash(buildParams, op, dependencies, globalUsage, buildUsage);
            if (TryLoadFromCache(buildParams.UseCache, hash, ref result))
            {
                if (!tracker.UpdateInfoUnchecked(string.Format("{0} : {1} (Cached)", bundleName, op.command.fileName)))
                    return BuildPipelineCodes.Canceled;

                SetOutputInformation(bundleName, result, output);
                return BuildPipelineCodes.SuccessCached;
            }

            if (!tracker.UpdateInfoUnchecked(string.Format("{0} : {1}", bundleName, op.command.fileName)))
                return BuildPipelineCodes.Canceled;

            result = op.Write(buildParams.GetTempOrCacheBuildPath(hash), dependencies, buildParams.BundleSettings, globalUsage, buildUsage);
            SetOutputInformation(bundleName, result, output);

            if (!TrySaveToCache(buildParams.UseCache, hash, result))
                BuildLogger.LogWarning("Unable to cache results for WriteCommand '{0}'.", op.command.internalName);
            return BuildPipelineCodes.Success;
        }

        protected static void SetOutputInformation(string bundleName, WriteResult result, IResultInfo output)
        {
            List<WriteResult> results;
            output.BundleResults.GetOrAdd(bundleName, out results);
            results.Add(result);
        }

        protected static bool TryLoadFromCache(bool useCache, Hash128 hash, ref WriteResult result)
        {
            WriteResult cachedResult;
            if (useCache && BuildCache.TryLoadCachedResults(hash, out cachedResult))
            {
                result = cachedResult;
                return true;
            }
            return false;
        }

        protected static bool TrySaveToCache(bool useCache, Hash128 hash, WriteResult result)
        {
            if (useCache && !BuildCache.SaveCachedResults(hash, result))
                return false;
            return true;
        }
    }
}
