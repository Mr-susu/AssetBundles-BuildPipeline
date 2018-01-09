using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Interfaces;
using UnityEditor.Experimental.Build.AssetBundle;

namespace UnityEditor.Build.Tasks
{
    public struct GenerateReleaseCommands : IBuildTask
    {
        // TODO: Move to utility file
        public const string k_UnityDefaultResourcePath = "library/unity default resources";

        const int k_Version = 1;
        public int Version { get { return k_Version; } }

        static readonly Type[] k_RequiredTypes = { typeof(IDeterministicIdentifiers), typeof(IBuildContent), typeof(IDependencyInfo), typeof(IPackingInfo), typeof(IWriteInfo) };
        public Type[] RequiredContextTypes { get { return k_RequiredTypes; } }

        public BuildPipelineCodes Run(IBuildContext context)
        {
            return Run(context.GetContextObject<IDeterministicIdentifiers>(), context.GetContextObject<IBuildContent>(), context.GetContextObject<IDependencyInfo>(),
                context.GetContextObject<IPackingInfo>(), context.GetContextObject<IWriteInfo>());
        }

        static BuildPipelineCodes Run(IDeterministicIdentifiers packingMethod, IBuildContent buildContent, IDependencyInfo dependencyInfo, IPackingInfo packingInfo, IWriteInfo writeInfo)
        {
            var commandsMap = CreateCommandsForFiles(packingMethod, packingInfo);

            return BuildPipelineCodes.Success;
        }

        static Dictionary<string, WriteCommand> CreateCommandsForFiles(IDeterministicIdentifiers packingMethod, IPackingInfo packingInfo)
        {
            var commandsMap = new Dictionary<string, WriteCommand>();

            foreach (var filePair in packingInfo.FileToObjects)
            {
                var command = new WriteCommand();
                command.fileName = packingMethod.GenerateInternalFileName(filePair.Key);
                command.internalName = string.Format("archive:/{0}/{0}", command.fileName); // TODO: Change this for upcoming Serialized File API
                //command.dependencies = op.info.bundleDependencies.Select(x => string.Format("archive:/{0}/{0}", packingMethod.GenerateInternalFileName(x))).ToList();
                command.serializeObjects = packingInfo.FileToObjects[filePair.Key].Select(x => new SerializationInfo
                {
                    serializationObject = x,
                    serializationIndex = packingMethod.SerializationIndexFromObjectIdentifier(x)
                }).ToList();

                commandsMap[filePair.Key] = command;
            }

            return commandsMap;
        }
    }
}
