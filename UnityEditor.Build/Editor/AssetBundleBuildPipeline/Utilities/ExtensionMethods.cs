using System.Collections.Generic;
using System.IO;

namespace UnityEditor.Build.Utilities
{
    public static class ExtensionMethods
    {
        public static bool IsNullOrEmpty<T> (this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static void GetOrAdd<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, out List<TValue> value)
        {
            if (dictionary.TryGetValue(key, out value))
                return;

            value = new List<TValue>();
            dictionary.Add(key, value);
        }

        public static void GetOrAdd<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> dictionary, TKey key, out HashSet<TValue> value)
        {
            if (dictionary.TryGetValue(key, out value))
                return;

            value = new HashSet<TValue>();
            dictionary.Add(key, value);
        }

        public static void Swap<T>(this IList<T> array, int index1, int index2)
        {
            var t = array[index2];
            array[index2] = array[index1];
            array[index1] = t;
        }

        public static void Swap<T>(this T[] array, int index1, int index2)
        {
            var t = array[index2];
            array[index2] = array[index1];
            array[index1] = t;
        }

        public static bool ValidScene(GUID asset)
        {
            var path = AssetDatabase.GUIDToAssetPath(asset.ToString());
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".unity") || !File.Exists(path))
                return false;
            return true;
        }

        public static bool ValidAsset(GUID asset)
        {
            var path = AssetDatabase.GUIDToAssetPath(asset.ToString());
            if (string.IsNullOrEmpty(path) || path.EndsWith(".unity") || !File.Exists(path))
                return false;
            return true;
        }
    }
}
