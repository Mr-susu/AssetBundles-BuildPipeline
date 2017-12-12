using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Tasks
{
    public class ConvertAssetBundleBuild
    {
        public static BuildPipelineCodes Run(AssetBundleBuild[] input, IBuildLayout output)
        {

            if (input.IsNullOrEmpty())
            {
                BuildLogger.LogError("Unable to continue. AssetBundleBuild input is null or empty!");
                return BuildPipelineCodes.Error;
            }

            var buildInput = new BuildInput();
            buildInput.definitions = new BuildInput.Definition[input.Length];
            for (var i = 0; i < input.Length; i++)
            {
                buildInput.definitions[i].assetBundleName = input[i].assetBundleName;
                buildInput.definitions[i].explicitAssets = new AssetIdentifier[input[i].assetNames.Length];
                for (var j = 0; j < input.Length; j++)
                {
                    var guid = AssetDatabase.AssetPathToGUID(input[i].assetNames[j]);
                    buildInput.definitions[i].explicitAssets[j].asset = new GUID(guid);
                    if (input[i].addressableNames.IsNullOrEmpty() || input[i].addressableNames.Length <= j || string.IsNullOrEmpty(input[i].addressableNames[j]))
                        buildInput.definitions[i].explicitAssets[j].address = input[i].assetNames[j];
                    else
                        buildInput.definitions[i].explicitAssets[j].address = input[i].addressableNames[j];
                }
            }

            output.Layout = buildInput;
            return BuildPipelineCodes.Success;
        }
    }
}