using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build
{
    public class BuildContext : IBuildContext
    {
        Dictionary<Type, IContextObject> m_ContextObjects;

        public BuildContext()
        {
            m_ContextObjects = new Dictionary<Type, IContextObject>();
        }

        public BuildContext(params IContextObject[] buildParams)
        {
            m_ContextObjects = new Dictionary<Type, IContextObject>();
            foreach (var buildParam in buildParams)
                SetContextObject(buildParam);
        }

        public void SetContextObject<T>(T contextObject) where T : IContextObject
        {
            m_ContextObjects[typeof(T)] = contextObject;
        }

        public void SetContextObject(IContextObject contextObject)
        {
            Type iCType = typeof(IContextObject);
            Type[] iTypes = contextObject.GetType().GetInterfaces();
            foreach (Type iType in iTypes)
            {
                if (!iCType.IsAssignableFrom(iType) || iType == iCType)
                    continue;
                m_ContextObjects[iType] = contextObject;
            }
        }

        public bool ContainsContextObject<T>() where T : IContextObject
        {
            return ContainsContextObject(typeof(T));
        }

        public bool ContainsContextObject(Type type)
        {
            return m_ContextObjects.ContainsKey(type);
        }

        public T GetContextObject<T>() where T : IContextObject
        {
            return (T)m_ContextObjects[typeof(T)];
        }

        public bool TryGetContextObject<T>(out T contextObject) where T : IContextObject
        {
            IContextObject cachedContext;
            if (m_ContextObjects.TryGetValue(typeof(T), out cachedContext) && cachedContext is T)
            {
                contextObject = (T)cachedContext;
                return true;
            }
            contextObject = default(T);
            return false;
        }
    }
}