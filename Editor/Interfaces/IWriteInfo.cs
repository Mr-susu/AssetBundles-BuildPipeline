using System.Collections.Generic;
using UnityEditor.Build.WriteTypes;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Interfaces
{
    public interface IWriteInfo : IContextObject
    {
        Dictionary<GUID, List<string>> AssetToFiles { get; }
        Dictionary<string, List<ObjectIdentifier>> FileToObjects { get; }
        Dictionary<string, WriteCommand> WriteCommands { get; }

        List<IWriteOperation> WriteOperations { get; }
    }

    public interface IBundleWriteInfo : IWriteInfo
    {
        Dictionary<string, string> FileToBundle { get; }
        Dictionary<string, BuildUsageTagSet> FileToUsageSet { get; }
        Dictionary<string, BuildReferenceMap> FileToReferenceMap { get; }
    }
}