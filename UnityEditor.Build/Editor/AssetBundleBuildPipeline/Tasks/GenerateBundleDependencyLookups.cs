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
            {
                BuildLogger.LogError("Running build pipeline that requires explicit bundle assignments without IBuildLayout.ExplicitLayout populated");
                return BuildPipelineCodes.Error;
            }

            foreach (KeyValuePair<string, List<GUID>> bundle in input.ExplicitLayout)
            {
                foreach (GUID asset in bundle.Value)
                {
                    // Add the current bundle as dependency[0]
                    var bundles = new List<string> { bundle.Key };
                    output.AssetToBundles.Add(asset, bundles);

                    // Add the current asset to the list of assets for a bundle
                    List<GUID> bundleAssets;
                    output.BundleToAssets.GetOrAdd(bundle.Key, out bundleAssets);
                    bundleAssets.Add(asset);
                }
            }
            return BuildPipelineCodes.Success;
        }
    }
}
