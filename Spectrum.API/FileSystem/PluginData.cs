using System;
using System.IO;

namespace Spectrum.API.FileSystem
{
    public class PluginData
    {
        public string DirectoryName { get; }
        public string DirectoryPath => Path.Combine(Defaults.PluginDataDirectory, DirectoryName);

        public PluginData(Type type)
        {
            DirectoryName = type.Assembly.GetName().Name;

            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
        }
    }
}
