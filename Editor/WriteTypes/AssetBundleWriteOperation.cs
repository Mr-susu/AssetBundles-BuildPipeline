using System;
using System.Collections.Generic;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.WriteTypes
{
    [Serializable]
    public class AssetBundleWriteOperation : RawWriteOperation
    {
        public AssetBundleInfo info { get { return m_Info; } }
        protected AssetBundleInfo m_Info = new AssetBundleInfo();

        public AssetBundleWriteOperation() { }
        public AssetBundleWriteOperation(RawWriteOperation other) : base(other) { }
        public AssetBundleWriteOperation(AssetBundleWriteOperation other) : base(other)
        {
            // Notes: May want to switch to MemberwiseClone, for now those this is fine
            m_Info = other.m_Info;
        }

        public override WriteResult Write(string outputFolder, List<WriteCommand> dependencies, BuildSettings settings, BuildUsageTagGlobal globalUsage, BuildUsageTagSet buildUsage)
        {
            var references = new BuildReferenceMap();
            references.AddMappings(command.internalName, command.serializeObjects.ToArray());
            if (!dependencies.IsNullOrEmpty())
            {
                foreach (var dependency in dependencies)
                    references.AddMappings(dependency.internalName, dependency.serializeObjects.ToArray());
            }

            return BundleBuildInterface.WriteSerializedFile(outputFolder, command, settings, globalUsage, buildUsage, references, info);
        }
    }
}
