using System.Collections.Generic;
using Mono.Cecil;
using Spectrum.Prism.IO;

namespace Spectrum.Prism.Runtime
{
    public class Patcher
    {
        private ModuleDefinition SourceModule { get; }
        private ModuleDefinition TargetModule { get; }

        private List<IPatch> Patches { get; }

        public Patcher(ModuleDefinition sourceModule, ModuleDefinition targetModule)
        {
            SourceModule = sourceModule;
            TargetModule = targetModule;

            Patches = new List<IPatch>();
        }

        public void AddPatch(IPatch patch)
        {
            if (patch == null)
                return;

            patch.PatchFailed += (sender, args) =>
            {
                ErrorHandler.TerminateWithError($"Patch '{args.Name}' failed. Reason: {args.Exception.Message}");
            };

            patch.PatchSucceeded += (sender, args) =>
            {
                ColoredOutput.WriteSuccess($"Patch '{args.Name}' succeeded.");
            };

            Patches.Add(patch);
        }

        public void RunAll()
        {
            foreach (var patch in Patches)
            {
                patch.Run(SourceModule, TargetModule);
            }
        }
    }
}
