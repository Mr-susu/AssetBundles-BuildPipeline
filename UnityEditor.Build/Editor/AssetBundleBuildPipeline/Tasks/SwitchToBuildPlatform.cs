namespace UnityEditor.Build
{
    public class SwitchToBuildPlatform : IBuildTask
    {
        protected const int k_Version = 1;
        public int Version { get { return k_Version; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildParams>());
        }

        public static BuildPipelineCodes Run(IBuildParams buildParams)
        {
            if (EditorUserBuildSettings.SwitchActiveBuildTarget(buildParams.Settings.group, buildParams.Settings.target))
                return BuildPipelineCodes.Success;
            return BuildPipelineCodes.Error;
        }
    }
}
