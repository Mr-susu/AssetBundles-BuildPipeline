using System;
using UnityEditor.Build.WriteTypes;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;

namespace UnityEditor.Build.Tasks
{
    public class WriteSerializedFiles : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IBuildParams), typeof(IDependencyInfo), typeof(IWriteInfo), typeof(IResultInfo) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            IProgressTracker tracker;
            context.TryGetContextObject(out tracker);
            return Run(context.GetContextObject<IBuildParams>(), context.GetContextObject<IDependencyInfo>(), context.GetContextObject<IWriteInfo>(), context.GetContextObject<IResultInfo>(), tracker);
        }

        protected static Hash128 CalculateInputHash(bool useCache, IWriteOperation operation, BuildSettings settings, BuildUsageTagGlobal globalUsage)
        {
            if (!useCache)
                return new Hash128();

            return HashingMethods.CalculateMD5Hash(k_Version, operation, settings, globalUsage);
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo dependencyInfo, IWriteInfo writeInfo, IResultInfo output, IProgressTracker tracker = null)
        {
            BuildUsageTagGlobal globalUSage = new BuildUsageTagGlobal();
            foreach (var sceneInfo in dependencyInfo.SceneInfo)
                globalUSage |= sceneInfo.Value.globalUsage;

            foreach (var op in writeInfo.WriteOperations)
            {
                Hash128 hash = CalculateInputHash(buildParams.UseCache, op, buildParams.BundleSettings, globalUSage);
                WriteResult result = new WriteResult();
                if (TryLoadFromCache(buildParams.UseCache, hash, ref result))
                {
                    if (!tracker.UpdateInfoUnchecked(string.Format("{0} (Cached)", op.command.internalName)))
                        return BuildPipelineCodes.Canceled;

                    SetOutputInformation(op.command.internalName, result, output);
                    continue;
                }

                result = op.Write(buildParams.GetTempOrCacheBuildPath(hash), buildParams.BundleSettings, globalUSage);
                SetOutputInformation(op.command.internalName, result, output);

                if (!TrySaveToCache(buildParams.UseCache, hash, result))
                    BuildLogger.LogWarning("Unable to cache WriteSerializedFiles result for file {0}.", op.command.internalName);
            }

            return BuildPipelineCodes.Success;
        }

        static void SetOutputInformation(string fileName, WriteResult result, IResultInfo output)
        {
            output.WriteResults.Add(fileName, result);
        }

        static bool TryLoadFromCache(bool useCache, Hash128 hash, ref WriteResult result)
        {
            WriteResult cachedResult;
            if (useCache && BuildCache.TryLoadCachedResults(hash, out cachedResult))
            {
                result = cachedResult;
                return true;
            }
            return false;
        }

        static bool TrySaveToCache(bool useCache, Hash128 hash, WriteResult result)
        {
            if (useCache && !BuildCache.SaveCachedResults(hash, result))
                return false;
            return true;
        }
    }
}
