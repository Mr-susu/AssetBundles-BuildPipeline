using System;
using System.Collections.Generic;
using UnityEditor.Build.WriteTypes;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build
{
    [Serializable]
    public class BuildWriteInfo : IWriteInfo
    {
        public Dictionary<string, IWriteOperation> AssetBundles { get; private set; }
        public Dictionary<string, List<IWriteOperation>> SceneBundles { get; private set; }

        public BuildWriteInfo()
        {
            AssetBundles = new Dictionary<string, IWriteOperation>();
            SceneBundles = new Dictionary<string, List<IWriteOperation>>();
        }
    }
}
