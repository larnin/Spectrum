using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Spectrum.Prism.Runtime;
using Spectrum.Prism.Runtime.EventArgs;

namespace Spectrum.Prism.Patches
{
    public class DevModeEnablePatch : BasePatch
    {
        public override string Name => "DevModeEnable";
        public override bool NeedsSource => false;

        public override void Run(ModuleDefinition moduleDefinition)
        {
            try
            {
                var targetType = moduleDefinition.GetType("GameManager");
                var methodDefinition = targetType.Methods.Single(m => m.Name == "get_IsDevBuild_");

                methodDefinition.Body.Instructions.Clear();
                var ilProcessor = methodDefinition.Body.GetILProcessor();

                var ldcInstruction = ilProcessor.Create(OpCodes.Ldc_I4_1);
                var retInstruction = ilProcessor.Create(OpCodes.Ret);

                ilProcessor.Append(ldcInstruction);
                ilProcessor.Append(retInstruction);

                OnPatchSucceeded(this, new PatchSucceededEventArgs(Name));
            }
            catch (Exception ex)
            {
                OnPatchFailed(this, new PatchFailedEventArgs(Name, ex));
            }
        }

        public override void Run(ModuleDefinition sourceModule, ModuleDefinition targetModule)
        {
            OnPatchFailed(this, new PatchFailedEventArgs(Name, new Exception("This patch does not require any source modules.")));
        }
    }
}
