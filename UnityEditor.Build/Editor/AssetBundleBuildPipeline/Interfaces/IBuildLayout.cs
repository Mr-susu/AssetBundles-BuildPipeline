using System.Collections.Generic;

namespace UnityEditor.Build.Interfaces
{
    public interface IBuildLayout : IContextObject
    {
        List<GUID> Assets { get; }

        List<GUID> Scenes { get; }

        Dictionary<GUID, string> Addresses { get; }

        Dictionary<string, List<GUID>> ExplicitLayout { get; }
    }
}