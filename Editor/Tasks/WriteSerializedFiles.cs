﻿using System;
using System.Collections.Generic;
using System.IO;
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

        protected static Hash128 CalculateInputHash(IBuildParams buildParams, IWriteOperation operation, List<WriteCommand> dependencies, BuildUsageTagGlobal globalUsage)
        {
            // TODO: Caching
            return new Hash128();
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams, IDependencyInfo dependencyInfo, IWriteInfo writeInfo, IResultInfo output, IProgressTracker tracker = null)
        {
            BuildUsageTagGlobal globalUSage = new BuildUsageTagGlobal();
            foreach (var sceneInfo in dependencyInfo.SceneInfo)
                globalUSage |= sceneInfo.Value.globalUsage;

            foreach (var op in writeInfo.WriteOperations)
            {
                Hash128 hash = CalculateInputHash(buildParams, op, null, globalUSage);
                WriteResult result = new WriteResult();
                if (TryLoadFromCache(buildParams.UseCache, hash, ref result))
                {
                    if (!tracker.UpdateInfoUnchecked(string.Format("{0} (Cached)", op.command.internalName)))
                        return BuildPipelineCodes.Canceled;

                    SetOutputInformation(op.command.internalName, result, output);
                    continue;
                }


                // TODO: Use temp location?
                result = op.Write(buildParams.OutputFolder, buildParams.BundleSettings, globalUSage);
                SetOutputInformation(op.command.internalName, result, output);
            }

            return BuildPipelineCodes.Success;
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

        static void SetOutputInformation(string fileName, WriteResult result, IResultInfo output)
        {
            output.WriteResults.Add(fileName, result);
        }
    }
}
