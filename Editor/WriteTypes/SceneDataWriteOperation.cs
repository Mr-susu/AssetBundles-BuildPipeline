using System;
using System.Collections.Generic;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.WriteTypes
{
    [Serializable]
    public class SceneDataWriteOperation : RawWriteOperation
    {
        public string scene = "";
        public string processedScene = "";
        public PreloadInfo preloadInfo = new PreloadInfo();
        public BuildUsageTagSet usageTags = new BuildUsageTagSet();

        public SceneDataWriteOperation() { }
        public SceneDataWriteOperation(RawWriteOperation other) : base(other) { }
        public SceneDataWriteOperation(SceneDataWriteOperation other) : base(other)
        {
            // Notes: May want to switch to MemberwiseClone, for now those this is fine
            scene = other.scene;
            processedScene = other.processedScene;
            preloadInfo = other.preloadInfo;
            usageTags = other.usageTags;
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

            buildUsage.UnionWith(usageTags);
            return BundleBuildInterface.WriteSceneSerializedFile(outputFolder, scene, processedScene, command, settings, globalUsage, buildUsage, references, preloadInfo);
        }
    }
}
