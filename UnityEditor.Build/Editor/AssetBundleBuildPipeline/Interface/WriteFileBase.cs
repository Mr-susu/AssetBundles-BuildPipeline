using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.AssetBundle.DataTypes;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build
{
    public abstract class WriteFileBase : IBuildTask<IDependencyInfo, IWriteInfo, IResultInfo>, IBuildTask
    {
        public virtual int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        protected abstract Hash128 CalculateInputHash(IWriteOperation operation, List<WriteCommand> dependencies, BuildUsageTagGlobal globalUsage, BuildUsageTagSet buildUsage);

        public virtual BuildPipelineCodes Run(object context)
        {
            return Run((IDependencyInfo)context, (IWriteInfo)context, (IResultInfo)context);
        }

        public abstract BuildPipelineCodes Run(IDependencyInfo input1, IWriteInfo input2, IResultInfo output);

        protected virtual BuildPipelineCodes WriteSerialziedFiles(string bundleName, IWriteOperation op, List<WriteCommand> allCommands, BuildUsageTagGlobal globalUsage, IResultInfo output)
        {
            var dependencies = op.CalculateDependencies(allCommands);

            var objectIDs = op.command.serializeObjects.Select(x => x.serializationObject).ToArray();
            var dependentIDs = dependencies.SelectMany(x => x.serializeObjects.Select(y => y.serializationObject)).ToArray();

            BuildUsageTagSet buildUsage = new BuildUsageTagSet();
            BundleBuildInterface.CalculateBuildUsageTags(objectIDs, dependentIDs, globalUsage, buildUsage);

            WriteResult result = new WriteResult();

            Hash128 hash = CalculateInputHash(op, dependencies, globalUsage, buildUsage);
            if (TryLoadFromCache(hash, ref result))
            {
                SetOutputInformation(bundleName, result, output);
                return BuildPipelineCodes.SuccessCached;
            }

            result = op.Write(BuildParams.GetTempOrCacheBuildPath(hash), dependencies, BuildParams.Settings, globalUsage, buildUsage);
            SetOutputInformation(bundleName, result, output);

            if (!TrySaveToCache(hash, result))
                BuildLogger.LogWarning("Unable to cache results for WriteCommand '{0}'.", op.command.internalName);
            return BuildPipelineCodes.Success;
        }

        protected virtual void SetOutputInformation(string bundleName, WriteResult result, IResultInfo output)
        {
            List<WriteResult> results;
            output.BundleResults.GetOrAdd(bundleName, out results);
            results.Add(result);
        }

        protected virtual bool TryLoadFromCache(Hash128 hash, ref WriteResult result)
        {
            WriteResult cachedResult;
            if (BuildParams.UseCache && BuildCache.TryLoadCachedResults(hash, out cachedResult))
            {
                result = cachedResult;
                return true;
            }
            return false;
        }

        protected virtual bool TrySaveToCache(Hash128 hash, WriteResult result)
        {
            if (BuildParams.UseCache && !BuildCache.SaveCachedResults(hash, result))
                return false;
            return true;
        }
    }
}
