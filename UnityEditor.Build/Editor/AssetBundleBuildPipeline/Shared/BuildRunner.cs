using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public static class BuildRunner
    {
        public static BuildPipelineCodes Run(IList<IBuildTask> pipeline, IBuildContext context)
        {
            foreach (IBuildTask task in pipeline)
            {
                try
                {
                    var result = task.Run(context);
                    if (result < BuildPipelineCodes.Success)
                        return result;
                }
                catch (System.Exception e)
                {
                    BuildLogger.LogException(e);
                    return BuildPipelineCodes.Exception;
                }
            }
            return BuildPipelineCodes.Success;
        }

        public static BuildPipelineCodes Validate(IList<IBuildTask> pipeline, IBuildContext context)
        {
            var requiredTypes = new HashSet<Type>();
            foreach (IBuildTask task in pipeline)
                requiredTypes.UnionWith(task.RequiredContextTypes);

            var missingTypes = new List<string>();
            foreach (Type requiredType in requiredTypes)
            {
                if (!context.ContainsContextObject(requiredType))
                    missingTypes.Add(requiredType.Name);
            }

            if (missingTypes.Count > 0)
            {
                BuildLogger.LogError("Missing required object types to run build pipeline:\n{0}", string.Join(", ", missingTypes.ToArray()));
                return BuildPipelineCodes.MissingRequiredObjects;
            }
            return BuildPipelineCodes.Success;
        }
    }
}