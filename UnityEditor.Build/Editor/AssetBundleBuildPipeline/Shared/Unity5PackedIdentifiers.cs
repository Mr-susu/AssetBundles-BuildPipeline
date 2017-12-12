using System;
using System.Text;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Utilities
{
    class Unity5PackedIdentifiers : IDeterministicIdentifiers
    {
        public string GenerateInternalFileName(string name)
        {
            var md4 = MD4.Create();
            var bytes = Encoding.ASCII.GetBytes(name);
            md4.TransformFinalBlock(bytes, 0, bytes.Length);
            return "CAB-" + BitConverter.ToString(md4.Hash, 0).ToLower().Replace("-", "");
        }

        public long SerializationIndexFromObjectIdentifier(ObjectIdentifier objectID)
        {
            byte[] bytes;
            var md4 = MD4.Create();
            if (objectID.fileType == FileType.MetaAssetType || objectID.fileType == FileType.SerializedAssetType)
            {
                // TODO: Variant info
                // NOTE: ToString() required as unity5 used the guid as a string to hash
                bytes = Encoding.ASCII.GetBytes(objectID.guid.ToString());
                md4.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                bytes = BitConverter.GetBytes((int)objectID.fileType);
                md4.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            }
            // Or path
            else
            {
                bytes = Encoding.ASCII.GetBytes(objectID.filePath);
                md4.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            }

            bytes = BitConverter.GetBytes(objectID.localIdentifierInFile);
            md4.TransformFinalBlock(bytes, 0, bytes.Length);
            var hash = BitConverter.ToInt64(md4.Hash, 0);
            return hash;
        }
    }
}