using System;
using System.Collections.Generic;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Interfaces
{
    public interface IPackingInfo : IContextObject
    {
        Dictionary<GUID, List<string>> AssetToFiles { get; }

        Dictionary<string, List<ObjectIdentifier>> FileToObjects { get; }
    }
}
