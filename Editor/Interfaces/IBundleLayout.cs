using System.Collections.Generic;

namespace UnityEditor.Build.Interfaces
{
    public interface IBundleLayout : IContextObject
    {
        // NOTES: Only used in Asset Bundle Compatible pipeline
        Dictionary<string, List<GUID>> ExplicitLayout { get; }
    }
}
