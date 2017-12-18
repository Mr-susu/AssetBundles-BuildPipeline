using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.WriteTypes
{
    [Serializable]
    public class RawWriteOperation : IWriteOperation
    {
        public WriteCommand command { get { return m_Command; } }
        protected WriteCommand m_Command = new WriteCommand();

        public RawWriteOperation() { }
        public RawWriteOperation(RawWriteOperation other)
        {
            // Notes: May want to switch to MemberwiseClone, for now those this is fine
            m_Command = other.m_Command;
        }

        public virtual List<WriteCommand> CalculateForwardDependencies(List<WriteCommand> allCommands)
        {
            if (command == null)
                return null;
            var results = allCommands.Where(x =>
            {
                if (!command.dependencies.IsNullOrEmpty() && command.dependencies.Contains(x.internalName))
                    return true;
                return false;
            });
            return results.ToList(); // TODO: Need to validate that we had all the dependencies
        }

        public virtual List<WriteCommand> CalculateReverseDependencies(List<WriteCommand> allCommands)
        {
            if (command == null)
                return null;
            var results = allCommands.Where(x =>
            {
                if (x != null && !x.dependencies.IsNullOrEmpty() && x.dependencies.Contains(command.internalName))
                    return true;
                return false;
            });
            return results.ToList(); // TODO: Need to validate that we had all the dependencies
        }

        public virtual WriteResult Write(string outputFolder, List<WriteCommand> dependencies, BuildSettings settings, BuildUsageTagGlobal globalUsage, BuildUsageTagSet buildUsage)
        {
            return BundleBuildInterface.WriteSerializedFile(outputFolder, command, dependencies, settings, globalUsage, buildUsage);
        }
    }
}
