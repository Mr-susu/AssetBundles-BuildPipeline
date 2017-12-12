using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Interfaces
{
    public interface IBundleInput : IContextObject
    {
        BuildInput BundleInput { get; }
    }
}