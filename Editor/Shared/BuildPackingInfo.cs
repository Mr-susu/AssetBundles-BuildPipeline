using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build
{
    [Serializable]
    public class BuildPackingInfo : IPackingInfo
    {
        public Dictionary<GUID, List<string>> AssetToFiles { get; private set; }
        public Dictionary<string, List<ObjectIdentifier>> FileToObjects { get; private set; }

        public BuildPackingInfo()
        {
            AssetToFiles = new Dictionary<GUID, List<string>>();
            FileToObjects = new Dictionary<string, List<ObjectIdentifier>>();
        }
    }
}
