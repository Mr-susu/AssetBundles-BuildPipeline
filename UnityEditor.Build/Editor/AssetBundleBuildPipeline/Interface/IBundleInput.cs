using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    public interface IBundleInput : IContextObject
    {
        BuildInput BundleInput { get; }
    }
}