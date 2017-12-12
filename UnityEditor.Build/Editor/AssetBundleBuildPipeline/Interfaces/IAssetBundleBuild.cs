namespace UnityEditor.Build.Interfaces
{
    public interface IAssetBundleBuild : IContextObject
    {
        AssetBundleBuild[] BundleBuild { get; set; }
    }
}
