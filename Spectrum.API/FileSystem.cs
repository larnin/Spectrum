using System;
using System.IO;
using Spectrum.API.Logging;

namespace Spectrum.API
{
    public class FileSystem
    {
        public string DirectoryName { get; }
        public string DirectoryPath => Path.Combine(Defaults.PluginDataDirectory, DirectoryName);

        private static Logger Log { get; set; }

        static FileSystem()
        {
            Log = new Logger(Defaults.FileSystemLogFileName);
        }

        public FileSystem(Type type)
        {
            DirectoryName = type.Assembly.GetName().Name;

            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
        }

        public string CreateFile(string fileName, bool overwrite = false)
        {
            var targetFilePath = Path.Combine(DirectoryPath, fileName);

            if (File.Exists(targetFilePath))
            {
                if (overwrite)
                {
                    RemoveFile(fileName);
                }
                else
                {
                    Log.Error($"Couldn't create a PluginData file for path '{targetFilePath}'. The file already exists.");
                    return string.Empty;
                }
            }

            try
            {
                File.Create(targetFilePath).Dispose();
                return targetFilePath;
            }
            catch (Exception ex)
            {
                Log.Error($"Couldn't create a PluginData file for path '{targetFilePath}'.");
                Log.Exception(ex);
                return string.Empty;
            }
        }

        public void RemoveFile(string fileName)
        {
            var targetFilePath = Path.Combine(DirectoryPath, fileName);

            if (!File.Exists(targetFilePath))
            {
                Log.Error($"Couldn't delete a PluginData file for path '{targetFilePath}'. File does not exist.");
                return;
            }

            try
            {
                File.Delete(targetFilePath);
            }
            catch (Exception ex)
            {
                Log.Error($"Couldn't delete a PluginData file for path '{targetFilePath}'.");
                Log.Exception(ex);
            }
        }

        public FileStream OpenFile(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            var targetFilePath = Path.Combine(DirectoryPath, fileName);

            if (!File.Exists(targetFilePath))
            {
                Log.Error($"Couldn't open the file. The requested file: '{targetFilePath}' does not exist.");
                return null;
            }

            try
            {
                return File.Open(targetFilePath, fileMode, fileAccess, fileShare);
            }
            catch (Exception ex)
            {
                Log.Error($"Couldn't open a PluginData file for path '{targetFilePath}'.");
                Log.Exception(ex);

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
                Log.Error($"Couldn't create a PluginData directory for path '{targetDirectoryPath}'.");
                Log.Exception(ex);
                return string.Empty;
            }
        }

        public void RemoveDirectory(string directoryName)
        {
            var targetDirectoryPath = Path.Combine(DirectoryPath, directoryName);

            if (!Directory.Exists(targetDirectoryPath))
            {
                Log.Error($"Couldn't remove a PluginData directory for path '{targetDirectoryPath}'. Directory does not exist.");
                return;
            }

            try
            {
                Directory.Delete(targetDirectoryPath, true);
            }
            catch (Exception ex)
            {
                Log.Error($"Couldn't remove a PluginData directory for path '{targetDirectoryPath}'.");
                Log.Exception(ex);
            }
        }
    }
}
