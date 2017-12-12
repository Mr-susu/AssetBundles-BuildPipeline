using System;
using System.Collections.Generic;
using UnityEditor.Build.AssetBundle;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build
{
    public class DefaultBuildContext : IBuildContext
    {
        Dictionary<Type, IContextObject> m_ContextObjects;

        public DefaultBuildContext(IBundleInput bundleInput, IBuildParams buildParams)
        {
            m_ContextObjects = new Dictionary<Type, IContextObject>();
            SetContextObject<IBundleInput>(bundleInput);
            SetContextObject<IBuildParams>(buildParams);

            SetContextObject<IDependencyInfo>(new DefaultBuildDependencyInfo());
            SetContextObject<IWriteInfo>(new DefaultBuildWriteInfo());
            SetContextObject<IResultInfo>(new DefaultBuildResultInfo());

            SetContextObject<IDeterministicIdentifiers>(new Unity5PackedIdentifiers());

            var callbacks = new DefaultBuildCallbacks();
            SetContextObject<IDependencyCallback>(callbacks);
            SetContextObject<IPackingCallback>(callbacks);
            SetContextObject<IWritingCallback>(callbacks);
        }

        public DefaultBuildContext(IBundleInput bundleInput, IBuildParams buildParams, IDependencyCallback dependencyCallback, IPackingCallback packingCallback, IWritingCallback writingCallback)
        {
            m_ContextObjects = new Dictionary<Type, IContextObject>();
            SetContextObject<IBundleInput>(bundleInput);
            SetContextObject<IBuildParams>(buildParams);

            SetContextObject<IDependencyInfo>(new DefaultBuildDependencyInfo());
            SetContextObject<IWriteInfo>(new DefaultBuildWriteInfo());
            SetContextObject<IResultInfo>(new DefaultBuildResultInfo());

            SetContextObject<IDeterministicIdentifiers>(new Unity5PackedIdentifiers());

            SetContextObject<IDependencyCallback>(dependencyCallback);
            SetContextObject<IPackingCallback>(packingCallback);
            SetContextObject<IWritingCallback>(writingCallback);
        }

        public void SetContextObject<T>(IContextObject contextObject) where T : IContextObject
        {
            m_ContextObjects[typeof(T)] = contextObject;
        }

        public T GetContextObject<T>() where T : IContextObject
        {
            return (T)m_ContextObjects[typeof(T)];
        }

        public bool TryGetContextObject<T>(out IContextObject contextObject) where T : IContextObject
        {
            return m_ContextObjects.TryGetValue(typeof(T), out contextObject);
        }
    }
}