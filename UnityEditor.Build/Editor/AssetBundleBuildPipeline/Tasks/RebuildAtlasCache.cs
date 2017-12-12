using UnityEditor.Build.Interfaces;
using UnityEditor.Sprites;

namespace UnityEditor.Build.Tasks
{
    public class RebuildAtlasCache : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>());
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams)
        {
            // TODO: Need a return value if this ever can fail
            Packer.RebuildAtlasCacheIfNeeded(buildParams.Settings.target, true, Packer.Execution.Normal);
            return BuildPipelineCodes.Success;
        }
    }
}
