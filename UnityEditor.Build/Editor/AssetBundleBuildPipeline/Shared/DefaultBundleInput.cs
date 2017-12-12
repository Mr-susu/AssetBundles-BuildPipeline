using UnityEditor.Build.Interfaces;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    public class DefaultBundleInput : IBundleInput
    {
        public BuildInput BundleInput { get; protected set; }

        public DefaultBundleInput(BuildInput bundleInput)
        {
            BundleInput = bundleInput;
        }
    }
}