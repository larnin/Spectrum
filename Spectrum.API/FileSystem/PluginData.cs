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

        public string CreateFile(string fileName)
        {
            var targetFilePath = Path.Combine(DirectoryPath, fileName);

            try
            {
                File.Create(targetFilePath).Dispose();
                return targetFilePath;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"API: Couldn't create a PluginData file for path {targetFilePath}. Exception below:\n{ex}");
                return "";
            }
        }

        public string CreateDirectory(string directoryName)
        {
            var targetDirectoryPath = Path.Combine(DirectoryPath, directoryName);

            try
            {
                Directory.CreateDirectory(targetDirectoryPath);
                return targetDirectoryPath;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"API: Couldn't create a PluginData directory for path {targetDirectoryPath}. Exception below:\n{ex}");
                return string.Empty;
            }
        }
    }
}
