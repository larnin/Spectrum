using System;

namespace Spectrum.Prism.Runtime.EventArgs
{
    public class PatchFailedEventArgs : System.EventArgs
    {
        public string Name { get; private set; }
        public Exception Exception { get; private set; }

        public PatchFailedEventArgs(string name, Exception exception)
        {
            Name = name;
            Exception = exception;
        }
    }
}
