using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public class DefaultBuildRunner
    {
        protected List<IBuildTask> m_Tasks = new List<IBuildTask>();

        public void AddTask(IBuildTask task)
        {
            m_Tasks.Add(task);
        }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            foreach (IBuildTask task in m_Tasks)
            {
                try
                {
                    var result = task.Run(context);
                    if (result < BuildPipelineCodes.Success)
                        return result;
                }
                catch (System.Exception e)
                {
                    BuildLogger.LogException(e);
                    return BuildPipelineCodes.Exception;
                }
            }
            return BuildPipelineCodes.Success;
        }
    }
}