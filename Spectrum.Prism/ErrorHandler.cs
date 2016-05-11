using System;
using Spectrum.Prism.IO;

namespace Spectrum.Prism
{
    class ErrorHandler
    {
        public static void TerminateWithError(string message)
        {
            ColoredOutput.WriteError(message);
            Environment.Exit(0);
        }
    }
}
