using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build.Tasks
{
    public class ValidateBundleAssignments : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { typeof(IDependencyInfo) };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IDependencyInfo>());
        }

        public static BuildPipelineCodes Run(IDependencyInfo input)
        {
            foreach (KeyValuePair<string, List<GUID>> bundle in input.BundleToAssets)
            {
                // TODO: Handle Player Data & Raw write formats
                if (ExtensionMethods.ValidAssetBundle(bundle.Value))
                    continue;

                if (ExtensionMethods.ValidSceneBundle(bundle.Value))
                    continue;

                BuildLogger.LogError("Bundle '{0}' contains mixed assets and scenes.", bundle.Key);
                return BuildPipelineCodes.Error;
            }

            return BuildPipelineCodes.Success;
        }
    }
}
