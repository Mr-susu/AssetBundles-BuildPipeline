using System.Diagnostics;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.Player;

namespace UnityEditor.Build.Player
{
    public static class PlayerBuildPipeline
    {
        public const string kTempPlayerBuildPath = "Temp/PlayerBuildData";

        public static ScriptBuildCallbacks BuildCallbacks = new ScriptBuildCallbacks();

        public static ScriptCompilationSettings GeneratePlayerBuildSettings()
        {
            var settings = new ScriptCompilationSettings();
            settings.target = EditorUserBuildSettings.activeBuildTarget;
            settings.group = UnityEditor.BuildPipeline.GetBuildTargetGroup(settings.target);
            settings.options = ScriptCompilationOptions.None;
            return settings;
        }

        public static ScriptCompilationSettings GeneratePlayerBuildSettings(BuildTarget target)
        {
            var settings = new ScriptCompilationSettings();
            settings.target = target;
            settings.group = UnityEditor.BuildPipeline.GetBuildTargetGroup(settings.target);
            settings.options = ScriptCompilationOptions.None;
            return settings;
        }

        public static ScriptCompilationSettings GeneratePlayerBuildSettings(BuildTarget target, BuildTargetGroup group)
        {
            var settings = new ScriptCompilationSettings();
            settings.target = target;
            settings.group = group;
            // TODO: Validate target & group
            settings.options = ScriptCompilationOptions.None;
            return settings;
        }

        public static ScriptCompilationSettings GeneratePlayerBuildSettings(BuildTarget target, BuildTargetGroup group, ScriptCompilationOptions options)
        {
            var settings = new ScriptCompilationSettings();
            settings.target = target;
            settings.group = group;
            // TODO: Validate target & group
            settings.options = options;
            return settings;
        }

        public static BuildPipelineCodes BuildPlayerScripts(ScriptCompilationSettings settings, string outputFolder, out BuildResultInfo result, bool useCache = true)
        {
            var buildTimer = new Stopwatch();
            buildTimer.Start();

            BuildPipelineCodes exitCode;
            result = new BuildResultInfo();

            using (var progressTracker = new ProgressTracker())
            {
                using (var buildCleanup = new BuildStateCleanup(false, kTempPlayerBuildPath))
                {
                    var buildParams = new BuildParams(settings, outputFolder, kTempPlayerBuildPath, useCache);

                    var buildContext = new BuildContext(buildParams, progressTracker);
                    buildContext.SetContextObject(result);
                    buildContext.SetContextObject(BuildCallbacks);

                    var pipeline = BuildPipeline.CreatePipeline(PipelineTasks.PlayerScriptsOnly);
                    exitCode = BuildRunner.Validate(pipeline, buildContext);
                    if (exitCode >= BuildPipelineCodes.Success)
                        exitCode = BuildRunner.Run(pipeline, buildContext);
                }
            }

            buildTimer.Stop();
            if (exitCode >= BuildPipelineCodes.Success)
                BuildLogger.Log("Build Player Scripts successful in: {0:c}", buildTimer.Elapsed);
            else if (exitCode == BuildPipelineCodes.Canceled)
                BuildLogger.LogWarning("Build Player Scripts canceled in: {0:c}", buildTimer.Elapsed);
            else
                BuildLogger.LogError("Build Player Scripts failed in: {0:c}", buildTimer.Elapsed);

            return exitCode;
        }
    }
}
