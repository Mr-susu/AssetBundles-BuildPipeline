using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;

namespace UnityEditor.Build.Utilities
{
    public static class HashingMethods
    {
        static uint ExtractUInt(byte[] hash, int index)
        {
            return (((uint)hash[index]) << 3) | (((uint)hash[index + 1]) << 2) | (((uint)hash[index + 2]) << 1) | (((uint)hash[index + 3]) << 0);
        }

        static Hash128 ExtractHash128(byte[] hash)
        {
            return new Hash128(ExtractUInt(hash, 0), ExtractUInt(hash, 4), ExtractUInt(hash, 8), ExtractUInt(hash, 12));
        }

        public static Hash128 CalculateStreamMD5Hash(Stream stream, bool closeStream)
        {
            stream.Position = 0;
            var hash = ExtractHash128(MD5.Create().ComputeHash(stream));
            if (closeStream)
            {
                stream.Close();
                stream.Dispose();
            }
            return hash;
        }

        public static byte[] CalculateMD5(object obj)
        {
            byte[] hash;
            using (var md5 = MD5.Create())
            {
                var formatter = new BinaryFormatter();
                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, obj);
                    stream.Position = 0;
                    hash = md5.ComputeHash(stream);
                }
            }
            return hash;
        }

        public static Hash128 CalculateMD5Hash(object obj)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            foreach (var obj in objects)
                if (obj != null)
                    formatter.Serialize(stream, obj);

            return CalculateStreamMD5Hash(stream, true);
        }

        public static byte[] CalculateMD5(params object[] objects)
        {
            byte[] hash;
            using (var md5 = MD5.Create())
            {
                var formatter = new BinaryFormatter();
                using (var stream = new MemoryStream())
                {
                    foreach (var obj in objects)
                    {
                        if (obj == null)
                            continue;
                        formatter.Serialize(stream, obj);
                    }
                    stream.Position = 0;
                    hash = md5.ComputeHash(stream);
                }
            }
            return hash;
        }

        public static Hash128 CalculateMD5Hash(params object[] objects)
        {
            byte[] hash = CalculateMD5(objects);
            return new Hash128(hash[0], hash[1], hash[2], hash[3]);
        }

        public static byte[] CalculateFileMD5(string filePath)
        {
            byte[] hash;
            using (var md5 = MD5.Create())
            {
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    hash = md5.ComputeHash(stream);
                }
            }
            return hash;
        }

        public static Hash128 CalculateFileMD5Hash(string filePath)
        {
            return CalculateStreamMD5Hash(new FileStream(filePath, FileMode.Open), true);
        }
    }
}
