using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build.Tasks
{
    public struct GenerateBundleDependencyLookups : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IBuildLayout), typeof(IDependencyInfo) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildLayout>(), context.GetContextObject<IDependencyInfo>());
        }

        public static BuildPipelineCodes Run(IBuildLayout input, IDependencyInfo output)
        {
            if (input.ExplicitLayout.IsNullOrEmpty())
                return BuildPipelineCodes.SuccessNotRun;

            foreach (KeyValuePair<string, List<GUID>> bundle in input.ExplicitLayout)
            {
                foreach (GUID asset in bundle.Value)
                {
                    // Add the current bundle as dependency[0]
                    var bundles = new List<string> { bundle.Key };
                    output.AssetToBundles.Add(asset, bundles);
                }
            }
            return BuildPipelineCodes.Success;
        }
    }
}
