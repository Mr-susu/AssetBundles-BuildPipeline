using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Interfaces
{
    public interface IBuildLayout : IContextObject
    {
        BuildInput Layout { get; set; }
    }
}