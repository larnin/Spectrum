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
            catch (Exception ex)
            {
                Console.WriteLine($"API: Couldn't create a PluginData file for path {targetFilePath}. Exception below:\n{ex}");
                return string.Empty;
            }
        }

        public FileStream OpenFile(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            var targetFilePath = Path.Combine(DirectoryPath, fileName);

            if (!File.Exists(targetFilePath))
            {
                Console.WriteLine($"API: The requested file: '{targetFilePath}' does not exist.");
                return null;
            }

            try
            {
                return File.Open(targetFilePath, fileMode, fileAccess, fileShare);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API: Couldn't open a PluginData file for path '{targetFilePath}'. Exception below:\n{ex}");
                return null;
            }
        }

        public FileStream OpenFile(string fileName)
        {
            return OpenFile(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        }

        public string CreateDirectory(string directoryName)
        {
            var targetDirectoryPath = Path.Combine(DirectoryPath, directoryName);

            try
            {
                Directory.CreateDirectory(targetDirectoryPath);
                return targetDirectoryPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API: Couldn't create a PluginData directory for path {targetDirectoryPath}. Exception below:\n{ex}");
                return string.Empty;
            }
        }
    }
}
