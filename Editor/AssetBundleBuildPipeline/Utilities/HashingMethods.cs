﻿using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;

namespace UnityEditor.Build.Utilities
{
    public static class HashingMethods
    {
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
            byte[] hash = CalculateMD5(obj);
            return new Hash128(hash[0], hash[1], hash[2], hash[3]);
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
            byte[] hash = CalculateFileMD5(filePath);
            return new Hash128(hash[0], hash[1], hash[2], hash[3]);
        }
    }
}