using UnityEditor.Build.Interfaces;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    public class BuildLayout : IBuildLayout
    {
        public BuildInput Layout { get; protected set; }

        public BuildLayout(BuildInput bundleInput)
        {
            Layout = bundleInput;
        }
    }
}