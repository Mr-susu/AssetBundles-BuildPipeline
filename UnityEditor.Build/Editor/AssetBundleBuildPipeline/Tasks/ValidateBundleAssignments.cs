using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public class ValidateBundleAssignments : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IDependencyInfo>());
        }

        public static BuildPipelineCodes Run(IDependencyInfo input)
        {
            foreach (KeyValuePair<string, List<GUID>> bundle in input.BundleToAssets)
            {
                // TODO: Handle Player Data & Raw write formats
                if (IsAssetBundle(bundle.Value))
                    continue;

                if (IsSceneBundle(bundle.Value))
                    continue;

                BuildLogger.LogError("Bundle '{0}' contains mixed assets and scenes.", bundle.Key);
                return BuildPipelineCodes.Error;
            }

            return BuildPipelineCodes.Success;
        }

        protected static bool IsAssetBundle(List<GUID> assets)
        {
            return assets.All(ExtensionMethods.ValidAsset);
        }

        protected static bool IsSceneBundle(List<GUID> assets)
        {
            return assets.All(ExtensionMethods.ValidScene);
        }
    }
}
