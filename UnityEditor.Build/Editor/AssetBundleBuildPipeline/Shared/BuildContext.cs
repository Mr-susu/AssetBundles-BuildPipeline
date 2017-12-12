using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;

namespace UnityEditor.Build
{
    public class BuildContext : IBuildContext
    {
        Dictionary<Type, IContextObject> m_ContextObjects;

        public BuildContext(IBuildLayout buildLayout, IBuildParams buildParams)
        {
            m_ContextObjects = new Dictionary<Type, IContextObject>();
            SetContextObject<IBuildLayout>(buildLayout);
            SetContextObject<IBuildParams>(buildParams);

            SetContextObject<IDependencyInfo>(new BuildDependencyInfo());
            SetContextObject<IWriteInfo>(new BuildWriteInfo());
            SetContextObject<IResultInfo>(new BuildResultInfo());

            SetContextObject<IDeterministicIdentifiers>(new Unity5PackedIdentifiers());

            var callbacks = new BuildCallbacks();
            SetContextObject<IDependencyCallback>(callbacks);
            SetContextObject<IPackingCallback>(callbacks);
            SetContextObject<IWritingCallback>(callbacks);
        }

        public BuildContext(IBuildLayout buildLayout, IBuildParams buildParams, IDependencyCallback dependencyCallback, IPackingCallback packingCallback, IWritingCallback writingCallback)
        {
            m_ContextObjects = new Dictionary<Type, IContextObject>();
            SetContextObject<IBuildLayout>(buildLayout);
            SetContextObject<IBuildParams>(buildParams);

            SetContextObject<IDependencyInfo>(new BuildDependencyInfo());
            SetContextObject<IWriteInfo>(new BuildWriteInfo());
            SetContextObject<IResultInfo>(new BuildResultInfo());

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