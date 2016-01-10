using System;
using System.IO;

namespace Spectrum.Manager.Logging
{
    class SubsystemLog
    {
        private string FilePath { get; }

        public SubsystemLog(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            FilePath = filePath;
        }

        public void Error(string message)
        {
            WriteLine($"[!][{DateTime.Now}] {message}");
        }

        public void Info(string message)
        {
            WriteLine($"[i][{DateTime.Now}] {message}");
        }

        public void Exception(Exception e)
        {
            WriteLine($"[e][{DateTime.Now}] {e.Message}");
            WriteLine($"   Target site: {e.TargetSite}");
            WriteLine("   Stack trace:");
            foreach (var s in e.StackTrace.Split('\n'))
            {
                WriteLine($"      {s}");
            }
        }

        private void WriteLine(string text)
        {
            using (var sw = new StreamWriter(FilePath, true))
            {
                sw.WriteLine(text);
            }
        }
    }
}
