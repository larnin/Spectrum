using System;

namespace Spectrum.API.Exceptions
{
    public class SettingsException : Exception
    {
        public string Key { get; }

        public SettingsException(string message, string key) : base(message)
        {
            Key = key;
        }
    }
}
