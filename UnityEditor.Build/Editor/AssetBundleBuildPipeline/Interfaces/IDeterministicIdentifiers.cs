using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Interfaces
{
    public interface IDeterministicIdentifiers
    {
        string GenerateInternalFileName(string name);
        long SerializationIndexFromObjectIdentifier(ObjectIdentifier objectID);
    }
}