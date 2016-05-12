using Mono.Cecil;

namespace Spectrum.Prism.IO
{
    public static class ModuleWriter
    {
        public static void SavePatchedFile(ModuleDefinition module, string fileName)
        {
            try
            {
                module.Write(fileName);
            }
            catch (AssemblyResolutionException)
            {
                ErrorHandler.TerminateWithError("Can't find the required dependencies. Make sure you run Prism inside the 'Managed' directory.");
            }
            catch
            {
                ErrorHandler.TerminateWithError("Can't write back the modified assembly. Is it in use and/or you don't have write rights?");
            }
        }
    }
}
