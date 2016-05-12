using System;
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
            catch(Exception ex)
            {
                ErrorHandler.TerminateWithError("Couldn't write back the patched file. Maybe it's in use?");
            }
        }
    }
}
