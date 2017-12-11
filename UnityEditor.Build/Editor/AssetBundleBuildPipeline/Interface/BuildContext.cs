using System;
using System.Collections.Generic;
using UnityEditor.Build.AssetBundle;
using UnityEditor.Build.AssetBundle.DataTypes;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    public interface IBundlesInput
    {
        BuildInput BundleInput { get; }
    }

    public interface IDependencyCallback
    {
        Func<IBuildParams, IDependencyInfo, BuildPipelineCodes> PostDependencyCallback { get; }
    }

    public interface IPackingCallback
    {
        Func<IBuildParams, IDependencyInfo, IWriteInfo, BuildPipelineCodes> PostPackingCallback { get; }
    }

    public interface IWritingCallback
    {
        Func<IBuildParams, IDependencyInfo, IWriteInfo, IResultInfo, BuildPipelineCodes> PostWritingCallback { get; }
    }

    public interface IDependencyInfo
    {
        Dictionary<GUID, AssetLoadInfo> AssetInfo { get; }
        Dictionary<GUID, SceneDependencyInfo> SceneInfo { get; }
        Dictionary<GUID, string> SceneAddress { get; }
        Dictionary<GUID, BuildUsageTagSet> SceneUsage { get; }
        BuildUsageTagGlobal GlobalUsage { get; set; }
        Dictionary<GUID, List<string>> AssetToBundles { get; }
        Dictionary<string, List<GUID>> BundleToAssets { get; }
    }

    public interface IWriteInfo
    {
        Dictionary<string, IWriteOperation> AssetBundles { get; }
        Dictionary<string, List<IWriteOperation>> SceneBundles { get; }
    }

    public interface IResultInfo
    {
        Dictionary<string, BundleInfo> BundleInfos { get; }
        Dictionary<string, List<WriteResult>> BundleResults { get; }
    }

    public class BuildContext : IBundlesInput, IDependencyInfo, IWriteInfo, IResultInfo, IDependencyCallback, IPackingCallback, IWritingCallback
    {
        public BuildInput BundleInput { get; set; }

        public Dictionary<GUID, AssetLoadInfo> AssetInfo { get { return m_DependencyData.assetInfo; } }
        public Dictionary<GUID, SceneDependencyInfo> SceneInfo { get { return m_DependencyData.sceneInfo; } }
        public Dictionary<GUID, string> SceneAddress { get { return m_DependencyData.sceneAddress; } }
        public Dictionary<GUID, BuildUsageTagSet> SceneUsage { get { return m_DependencyData.sceneUsageTags; } }
        public BuildUsageTagGlobal GlobalUsage
        {
            get { return m_DependencyData.buildGlobalUsage; }
            set { m_DependencyData.buildGlobalUsage = value; }
        }
        public Dictionary<GUID, List<string>> AssetToBundles { get { return m_DependencyData.assetToBundles; } }
        public Dictionary<string, List<GUID>> BundleToAssets { get { return m_DependencyData.bundleToAssets; } }
        public Dictionary<string, IWriteOperation> AssetBundles { get { return m_WriteInfo.assetBundles; } }
        public Dictionary<string, List<IWriteOperation>> SceneBundles { get { return m_WriteInfo.sceneBundles; } }
        public Dictionary<string, BundleInfo> BundleInfos { get { return m_ResultInfo.bundleInfos; } }
        public Dictionary<string, List<WriteResult>> BundleResults { get { return m_ResultInfo.bundleResults; } }

        public Func<IBuildParams, IDependencyInfo, BuildPipelineCodes> PostDependencyCallback { get; set; }
        public Func<IBuildParams, IDependencyInfo, IWriteInfo, BuildPipelineCodes> PostPackingCallback { get; set; }
        public Func<IBuildParams, IDependencyInfo, IWriteInfo, IResultInfo, BuildPipelineCodes> PostWritingCallback { get; set; }

        // TODO: Replace these classes maybe?
        private BuildDependencyInfo m_DependencyData = new BuildDependencyInfo();
        private BuildWriteInfo m_WriteInfo = new BuildWriteInfo();
        private BuildResultInfo m_ResultInfo = new BuildResultInfo();
    }
}
