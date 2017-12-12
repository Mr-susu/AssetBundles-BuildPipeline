using UnityEditor.Build.Interfaces;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    public class BuildLayout : IBuildLayout
    {
        public BuildInput Layout { get; set; }

        internal BuildLayout() { }

        public BuildLayout(BuildInput bundleInput)
        {
            Layout = bundleInput;
        }
    }
}