using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    [Serializable]
    public class BuildContent : IBuildContent, IBuildLayout
    {
        public List<GUID> Assets { get; private set; }

        public List<GUID> Scenes { get; private set; }

        public Dictionary<GUID, string> Addresses { get; private set; }

        // TODO: Unused in Auto-Packing pipeline
        public Dictionary<string, List<GUID>> ExplicitLayout { get; private set; }

        public BuildContent(BuildInput bundleInput)
        {
            Assets = new List<GUID>();
            Scenes = new List<GUID>();
            Addresses = new Dictionary<GUID, string>();
            ExplicitLayout = new Dictionary<string, List<GUID>>();

            foreach (var bundle in bundleInput.definitions)
            {
                List<GUID> guids;
                ExplicitLayout.GetOrAdd(bundle.assetBundleName, out guids);
                foreach (var assetInfo in bundle.explicitAssets)
                {
                    guids.Add(assetInfo.asset);
                    Addresses.Add(assetInfo.asset, string.IsNullOrEmpty(assetInfo.address) ? AssetDatabase.GUIDToAssetPath(assetInfo.asset.ToString()) : assetInfo.address);
                    if (ExtensionMethods.ValidAsset(assetInfo.asset))
                        Assets.Add(assetInfo.asset);
                    else if (ExtensionMethods.ValidScene(assetInfo.asset))
                        Scenes.Add(assetInfo.asset);
                    else
                        throw new ArgumentException(string.Format("Asset '{0}' is not a valid Asset or Scene.", assetInfo.asset));
                }
            }
        }

        public BuildContent(IEnumerable<GUID> assets)
        {
            Assets = new List<GUID>();
            Scenes = new List<GUID>();
            Addresses = new Dictionary<GUID, string>();

            foreach (var asset in assets)
            {
                Addresses.Add(asset, AssetDatabase.GUIDToAssetPath(asset.ToString()));
                if (ExtensionMethods.ValidAsset(asset))
                    Assets.Add(asset);
                else if (ExtensionMethods.ValidScene(asset))
                    Scenes.Add(asset);
                else
                    throw new ArgumentException(string.Format("Asset '{0}' is not a valid Asset or Scene.", asset));
            }
        }

        public BuildContent(IEnumerable<AssetBundleBuild> bundleBuilds)
        {
            Assets = new List<GUID>();
            Scenes = new List<GUID>();
            Addresses = new Dictionary<GUID, string>();
            ExplicitLayout = new Dictionary<string, List<GUID>>();

            foreach (var bundleBuild in bundleBuilds)
            {
                List<GUID> guids;
                ExplicitLayout.GetOrAdd(bundleBuild.assetBundleName, out guids);

                for (var i = 0; i < bundleBuild.assetNames.Length; i++)
                {
                    var asset = new GUID(AssetDatabase.AssetPathToGUID(bundleBuild.assetNames[i]));
                    var address = i >= bundleBuild.addressableNames.Length || string.IsNullOrEmpty(bundleBuild.addressableNames[i]) ?
                        AssetDatabase.GUIDToAssetPath(asset.ToString()) : bundleBuild.addressableNames[i];
                    Addresses.Add(asset, address);
                    if (ExtensionMethods.ValidAsset(asset))
                        Assets.Add(asset);
                    else if (ExtensionMethods.ValidScene(asset))
                        Scenes.Add(asset);
                    else
                        throw new ArgumentException(string.Format("Asset '{0}' is not a valid Asset or Scene.", bundleBuild.assetNames[i]));
                }
            }
        }
    }
}