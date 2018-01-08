using System.Collections.Generic;

namespace UnityEditor.Build.Interfaces
{
    // Rename from BuildContent
    public interface IBuildContent : IContextObject
    {
        List<GUID> Assets { get; }

        List<GUID> Scenes { get; }

        Dictionary<GUID, string> Addresses { get; }
    }
}