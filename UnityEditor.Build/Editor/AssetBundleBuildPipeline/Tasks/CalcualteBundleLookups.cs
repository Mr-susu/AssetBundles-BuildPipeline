using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Tasks
{
    public class CalculateBundleLookups : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildLayout>(), context.GetContextObject<IDependencyInfo>());
        }

        public BuildPipelineCodes Run(IBuildLayout input, IDependencyInfo output)
        {
            foreach (BuildInput.Definition bundle in input.Layout.definitions)
            {
                foreach (AssetIdentifier assetID in bundle.explicitAssets)
                {
                    // Add the current bundle as dependency[0]
                    var bundles = new List<string>();
                    bundles.Add(bundle.assetBundleName);
                    output.AssetToBundles.Add(assetID.asset, bundles);

                    // Add the current asset to the list of assets for a bundle
                    List<GUID> bundleAssets;
                    output.BundleToAssets.GetOrAdd(bundle.assetBundleName, out bundleAssets);
                    bundleAssets.Add(assetID.asset);
                }
            }
            return BuildPipelineCodes.Success;
        }
    }
}
