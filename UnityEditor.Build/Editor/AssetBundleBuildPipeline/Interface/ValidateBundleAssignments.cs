using System.Collections.Generic;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public class ValidateBundleAssignments : IBuildTask<IDependencyInfo>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run((IDependencyInfo)context);
        }

        public BuildPipelineCodes Run(IDependencyInfo input)
        {
            foreach (var bundle in input.BundleToAssets)
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

        protected bool IsAssetBundle(List<GUID> assets)
        {
            foreach (var asset in assets)
            {
                if (ExtensionMethods.ValidAsset(asset))
                    continue;
                return false;
            }
            return true;
        }

        protected bool IsSceneBundle(List<GUID> assets)
        {
            foreach (var asset in assets)
            {
                if (ExtensionMethods.ValidScene(asset))
                    continue;
                return false;
            }
            return true;
        }
    }
}
