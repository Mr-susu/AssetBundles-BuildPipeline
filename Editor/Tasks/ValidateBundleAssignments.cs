﻿using System;
using System.Collections.Generic;
using UnityEditor.Build.Interfaces;
using UnityEditor.Build.Utilities;

namespace UnityEditor.Build.Tasks
{
    public struct ValidateBundleAssignments : IBuildTask
    {
        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IBuildLayout) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IBuildLayout>());
        }

        public static BuildPipelineCodes Run(IBuildLayout input)
        {
            if (input.ExplicitLayout.IsNullOrEmpty())
                return BuildPipelineCodes.Success;

            foreach (KeyValuePair<string, List<GUID>> bundle in input.ExplicitLayout)
            {
                // TODO: Handle Player Data & Raw write formats
                if (ExtensionMethods.ValidAssetBundle(bundle.Value))
                    continue;

                if (ExtensionMethods.ValidSceneBundle(bundle.Value))
                    continue;

                BuildLogger.LogError("Bundle '{0}' contains mixed assets and scenes.", bundle.Key);
                return BuildPipelineCodes.Error;
            }

            return BuildPipelineCodes.Success;
        }
    }
}