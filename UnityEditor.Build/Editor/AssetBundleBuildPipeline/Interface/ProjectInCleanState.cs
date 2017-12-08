using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public class ProjectInCleanState : IBuildTask<object, object>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run(context, context);
        }

        public BuildPipelineCodes Run(object input, object output)
        {
            if (ProjectValidator.HasDirtyScenes())
                return BuildPipelineCodes.Success;
            return BuildPipelineCodes.UnsavedChanges;
        }
    }
}
