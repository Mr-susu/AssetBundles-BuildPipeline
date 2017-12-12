﻿using System;
using System.Collections.Generic;
using UnityEditor.Build.AssetBundle.DataTypes;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build
{
    [Serializable]
    public class DefaultBuildWriteInfo : IWriteInfo
    {
        public Dictionary<string, IWriteOperation> AssetBundles { get; private set; }
        public Dictionary<string, List<IWriteOperation>> SceneBundles { get; private set; }

        public DefaultBuildWriteInfo()
        {
            AssetBundles = new Dictionary<string, IWriteOperation>();
            SceneBundles = new Dictionary<string, List<IWriteOperation>>();
        }
    }
}
