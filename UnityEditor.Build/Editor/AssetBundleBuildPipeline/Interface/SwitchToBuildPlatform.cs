namespace UnityEditor.Build
{
    public class SwitchToBuildPlatform : IBuildTask<object, object>, IBuildTask
    {
        public int Version { get { return 1; } }

        public IBuildParams BuildParams { get; set; }

        public BuildPipelineCodes Run(object context)
        {
            return Run(context, context);
        }

        public BuildPipelineCodes Run(object input, object output)
        {
            if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildParams.Settings.group, BuildParams.Settings.target))
                return BuildPipelineCodes.Success;
            return BuildPipelineCodes.Error;
        }
    }
}
