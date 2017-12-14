namespace UnityEditor.Build.Interfaces
{
    public interface IProgressTracker : IContextObject
    {
        int TaskCount { get; set; }

        float Progress { get; }

        bool UpdateTask(string taskTitle);

        bool UpdateInfo(string info);
    }
}
