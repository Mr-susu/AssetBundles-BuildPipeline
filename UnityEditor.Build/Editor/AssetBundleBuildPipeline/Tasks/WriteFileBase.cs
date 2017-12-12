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
        public virtual int Version { get { return 1; } }

        public virtual BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IDependencyInfo>(), context.GetContextObject<IWriteInfo>(), context.GetContextObject<IResultInfo>());
        }

        protected abstract Hash128 CalculateInputHash(IBuildParams buildParams, IWriteOperation operation, List<WriteCommand> dependencies, BuildUsageTagGlobal globalUsage, BuildUsageTagSet buildUsage);

        public abstract BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo input1, IWriteInfo input2, IResultInfo output);

        protected virtual BuildPipelineCodes WriteSerialziedFiles(IBuildParams buildParams, string bundleName, IWriteOperation op, List<WriteCommand> allCommands, BuildUsageTagGlobal globalUsage, IResultInfo output)
        {
            List<WriteCommand> dependencies = op.CalculateDependencies(allCommands);

            ObjectIdentifier[] objectIDs = op.command.serializeObjects.Select(x => x.serializationObject).ToArray();
            ObjectIdentifier[] dependentIDs = dependencies.SelectMany(x => x.serializeObjects.Select(y => y.serializationObject)).ToArray();

            var buildUsage = new BuildUsageTagSet();
            BundleBuildInterface.CalculateBuildUsageTags(objectIDs, dependentIDs, globalUsage, buildUsage);

            var result = new WriteResult();

            Hash128 hash = CalculateInputHash(buildParams, op, dependencies, globalUsage, buildUsage);
            if (TryLoadFromCache(buildParams.UseCache, hash, ref result))
            {
                SetOutputInformation(bundleName, result, output);
                return BuildPipelineCodes.SuccessCached;
            }

            result = op.Write(buildParams.GetTempOrCacheBuildPath(hash), dependencies, buildParams.Settings, globalUsage, buildUsage);
            SetOutputInformation(bundleName, result, output);

            if (!TrySaveToCache(buildParams.UseCache, hash, result))
                BuildLogger.LogWarning("Unable to cache results for WriteCommand '{0}'.", op.command.internalName);
            return BuildPipelineCodes.Success;
        }

        protected virtual void SetOutputInformation(string bundleName, WriteResult result, IResultInfo output)
        {
            List<WriteResult> results;
            output.BundleResults.GetOrAdd(bundleName, out results);
            results.Add(result);
        }

        protected virtual bool TryLoadFromCache(bool useCache, Hash128 hash, ref WriteResult result)
        {
            WriteResult cachedResult;
            if (useCache && BuildCache.TryLoadCachedResults(hash, out cachedResult))
            {
                result = cachedResult;
                return true;
            }
            return false;
        }

        protected virtual bool TrySaveToCache(bool useCache, Hash128 hash, WriteResult result)
        {
            if (useCache && !BuildCache.SaveCachedResults(hash, result))
                return false;
            return true;
        }
    }
}
