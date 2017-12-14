using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Tasks
{
    public class GenerateAssetToBundleDependency : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        protected static Type[] s_RequiredTypes = { typeof(IDependencyInfo) };
        public Type[] RequiredContextTypes { get { return s_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IDependencyInfo>());
        }

        public BuildPipelineCodes Run(IDependencyInfo dependencyInfo)
        {
            // Generate the explicit asset to bundle dependency lookup 
            foreach (var asset in dependencyInfo.AssetInfo.Values)
            {
                var assetBundles = dependencyInfo.AssetToBundles[asset.asset];
                foreach (var reference in asset.referencedObjects)
                {
                    List<string> refBundles;
                    if (!dependencyInfo.AssetToBundles.TryGetValue(reference.guid, out refBundles))
                        continue;

                    var dependency = refBundles[0];
                    if (assetBundles.Contains(dependency))
                        continue;

                    assetBundles.Add(dependency);
                }
            }
            return BuildPipelineCodes.Success;
        }
    }
}
