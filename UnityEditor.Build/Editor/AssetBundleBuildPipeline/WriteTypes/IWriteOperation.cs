using System.Collections.Generic;
using UnityEditor.Experimental.Build;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.WriteTypes
{
    public interface IWriteOperation
    {
        WriteCommand command { get; }

        List<WriteCommand> CalculateForwardDependencies(List<WriteCommand> allCommands);

        List<WriteCommand> CalculateReverseDependencies(List<WriteCommand> allCommands);

        WriteResult Write(string outputFolder, List<WriteCommand> dependencies, BuildSettings settings, BuildUsageTagGlobal globalUsage, BuildUsageTagSet buildUsage);
    }
}
