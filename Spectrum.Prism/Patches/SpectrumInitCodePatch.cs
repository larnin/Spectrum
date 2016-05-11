using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Spectrum.Prism.Runtime;
using Spectrum.Prism.Runtime.EventArgs;

namespace Spectrum.Prism.Patches
{
    public class SpectrumInitCodePatch : BasePatch
    {
        public override string Name => "Initialization patch";

        public override void Run(ModuleDefinition sourceModule, ModuleDefinition targetModule)
        {
            try
            {
                var targetMethod = PatchHelper.FindInitializationMethodDefinition(targetModule);
                var ilProcessor = targetMethod.Body.GetILProcessor();

                var initMethodReference = PatchHelper.ImportBootstrapMethodReference(targetModule, sourceModule);
                var initializationInstruction = ilProcessor.Create(OpCodes.Call, initMethodReference);

                var lastAwakeInstruction = ilProcessor.Body.Instructions[ilProcessor.Body.Instructions.Count - 2];

                if (lastAwakeInstruction.OpCode == OpCodes.Call)
                {
                    var eventArgs = new PatchFailedEventArgs(Name, new Exception("This patch has already been applied."));
                    OnPatchFailed(this, eventArgs);

                    return;
                }
                ilProcessor.InsertAfter(lastAwakeInstruction, initializationInstruction);
                OnPatchSucceeded(this, new PatchSucceededEventArgs(Name));
            }
            catch (Exception ex)
            {
                var eventArgs = new PatchFailedEventArgs(Name, ex);
                OnPatchFailed(this, eventArgs);
            }
        }
    }
}
