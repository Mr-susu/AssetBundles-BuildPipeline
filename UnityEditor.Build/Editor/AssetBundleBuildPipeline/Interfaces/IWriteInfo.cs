using System.Collections.Generic;
using UnityEditor.Build.AssetBundle.DataTypes;

namespace UnityEditor.Build.Interfaces
{
    public interface IWriteInfo : IContextObject
    {
        Dictionary<string, IWriteOperation> AssetBundles { get; }
        Dictionary<string, List<IWriteOperation>> SceneBundles { get; }
    }
}