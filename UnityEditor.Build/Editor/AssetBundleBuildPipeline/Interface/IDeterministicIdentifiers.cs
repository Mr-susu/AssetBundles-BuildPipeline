using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    public interface IDeterministicIdentifiers : IContextObject
    {
        string GenerateInternalFileName(string name);
        long SerializationIndexFromObjectIdentifier(ObjectIdentifier objectID);
    }
}