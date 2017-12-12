using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Tasks
{
    public class ConvertAssetBundleBuild : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IAssetBundleBuild>(), context.GetContextObject<IBuildLayout>());
        }

        public static BuildPipelineCodes Run(IAssetBundleBuild input, IBuildLayout output)
        {
            AssetBundleBuild[] bundles = input.BundleBuild;
            if (bundles.IsNullOrEmpty())
            {
                BuildLogger.LogError("Unable to continue. AssetBundleBuild input is null or empty!");
                return BuildPipelineCodes.Error;
            }

            var buildInput = new BuildInput();
            buildInput.definitions = new BuildInput.Definition[bundles.Length];
            for (var i = 0; i < bundles.Length; i++)
            {
                buildInput.definitions[i].assetBundleName = bundles[i].assetBundleName;
                buildInput.definitions[i].explicitAssets = new AssetIdentifier[bundles[i].assetNames.Length];
                for (var j = 0; j < bundles.Length; j++)
                {
                    var guid = AssetDatabase.AssetPathToGUID(bundles[i].assetNames[j]);
                    buildInput.definitions[i].explicitAssets[j].asset = new GUID(guid);
                    if (bundles[i].addressableNames.IsNullOrEmpty() || bundles[i].addressableNames.Length <= j || string.IsNullOrEmpty(bundles[i].addressableNames[j]))
                        buildInput.definitions[i].explicitAssets[j].address = bundles[i].assetNames[j];
                    else
                        buildInput.definitions[i].explicitAssets[j].address = bundles[i].addressableNames[j];
                }
            }

            output.Layout = buildInput;
            return BuildPipelineCodes.Success;
        }
    }
}