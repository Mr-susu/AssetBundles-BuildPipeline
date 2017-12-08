using UnityEditor.Sprites;

namespace UnityEditor.Build
{
    public class RebuildAtlasCache : IBuildTask<object, object>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run(context, context);
        }

        public BuildPipelineCodes Run(object input, object output)
        {
            // TODO: Need a return value if this ever can fail
            Packer.RebuildAtlasCacheIfNeeded(BuildParams.Settings.target, true, Packer.Execution.Normal);
            return BuildPipelineCodes.Success;
        }
    }
}
