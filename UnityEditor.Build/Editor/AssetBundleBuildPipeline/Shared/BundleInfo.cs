using System;
using UnityEngine;

namespace UnityEditor.Build
{
    [Serializable]
    public struct BundleInfo
    {
        public string name;
        public uint crc;
        public Hash128 hash;
    }
}