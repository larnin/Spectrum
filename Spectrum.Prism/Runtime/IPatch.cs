using System;
using Mono.Cecil;
using Spectrum.Prism.Runtime.EventArgs;

namespace Spectrum.Prism.Runtime
{
    public interface IPatch
    {
        string Name { get; }

        event EventHandler<PatchFailedEventArgs> PatchFailed;
        event EventHandler<PatchSucceededEventArgs> PatchSucceeded;

        void Run(ModuleDefinition moduleDefinition);
        void Run(ModuleDefinition sourceModule, ModuleDefinition targetModule);
    }
}
