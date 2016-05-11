using System;
using Mono.Cecil;

namespace Spectrum.Prism.IO
{
    public class ModuleLoader
    {
        public static ModuleDefinition LoadDistanceModule(string distanceAssemblyFilename)
        {
            try
            {
                ColoredOutput.WriteInformation("Loading TARGET module...");
                return ModuleDefinition.ReadModule(distanceAssemblyFilename);
            }
            catch (Exception e)
            {
                ErrorHandler.TerminateWithError($"Couldn't load TARGET module definition. Exception details:\n{e}");
            }
            return null;
        }

        public static ModuleDefinition LoadBootstrapModule(string bootstrapAssemblyFilename)
        {
            try
            {
                ColoredOutput.WriteInformation("Loading BOOTSTRAP module...");
                return ModuleDefinition.ReadModule(bootstrapAssemblyFilename);
            }
            catch (Exception e)
            {
                ErrorHandler.TerminateWithError($"Could't load BOOTSTRAP module definition. Exception details:\n{e}");
            }
            return null;
        }
    }
}
