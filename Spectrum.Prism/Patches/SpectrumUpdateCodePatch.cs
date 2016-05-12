using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Spectrum.Prism.Runtime;
using Spectrum.Prism.Runtime.EventArgs;

namespace Spectrum.Prism.Patches
{
    public class SpectrumUpdateCodePatch : BasePatch
    {
        public override string Name => "SpectrumStateUpdate";
        public override bool NeedsSource => true;

        public override void Run(ModuleDefinition sourceModule, ModuleDefinition targetModule)
        {
            try
            {
                var targetMethod = PatchHelper.FindUpdateMethodDefinition(targetModule);
                var ilProcessor = targetMethod.Body.GetILProcessor();

                var updateMethodReference = PatchHelper.ImportUpdateMethodReference(targetModule, sourceModule);
                var updateInstruction = ilProcessor.Create(OpCodes.Call, updateMethodReference);

                var originalReturnInstruction = ilProcessor.Body.Instructions[ilProcessor.Body.Instructions.Count - 1];

                if (originalReturnInstruction.OpCode == OpCodes.Call)
                {
                    var eventArgs = new PatchFailedEventArgs(Name, new Exception("This patch has already been applied."));
                    OnPatchFailed(this, eventArgs);
                }

                updateInstruction.Offset = originalReturnInstruction.Offset;

                ilProcessor.Replace(originalReturnInstruction, updateInstruction);

                var returnInstruction = ilProcessor.Create(OpCodes.Ret);
                ilProcessor.InsertAfter(updateInstruction, returnInstruction);

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
