using System.Collections.Generic;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public class CalculateBundleLookups : IBuildTask<IBundlesInput, IDependencyInfo>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run((IBundlesInput)context, (IDependencyInfo)context);
        }

        public BuildPipelineCodes Run(IBundlesInput input, IDependencyInfo output)
        {
            foreach (var bundle in input.BundleInput.definitions)
            {
                foreach (var assetID in bundle.explicitAssets)
                {
                    // Add the current bundle as dependency[0]
                    List<string> bundles = new List<string>();
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
