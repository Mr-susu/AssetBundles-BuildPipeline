using System;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build.Utilities
{
    public class BuildProgressTracker : IProgressTracker, IDisposable
    {
        public int TaskCount { get; set; }

        public float Progress { get { return m_CurrentTask / (float)TaskCount; } }

        protected int m_CurrentTask = 0;

        protected string m_CurrentTaskTitle = "";

        public bool UpdateTask(string taskTitle)
        {
            m_CurrentTask++;
            m_CurrentTaskTitle = taskTitle;
            return !EditorUtility.DisplayCancelableProgressBar(m_CurrentTaskTitle, "", Progress);
        }

        public bool UpdateInfo(string info)
        {
            return !EditorUtility.DisplayCancelableProgressBar(m_CurrentTaskTitle, info, Progress);
        }

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
