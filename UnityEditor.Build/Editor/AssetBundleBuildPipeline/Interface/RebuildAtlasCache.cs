using UnityEditor.Sprites;

namespace UnityEditor.Build
{
    public class RebuildAtlasCache : IBuildTask<object>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        BuildPipelineCodes IBuildTask.Run(object context)
        {
            return RunPacker();
        }

        BuildPipelineCodes IBuildTask<object>.Run(object context)
        {
            return RunPacker();
        }

        private BuildPipelineCodes RunPacker()
        {
            // TODO: Need a return value if this ever can fail
            Packer.RebuildAtlasCacheIfNeeded(BuildParams.Settings.target, true, Packer.Execution.Normal);
            return BuildPipelineCodes.Success;
        }
    }
}
