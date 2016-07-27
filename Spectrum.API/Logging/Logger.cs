using System;
using System.IO;

namespace Spectrum.API.Logging
{
    public class Logger
    {
        public bool WriteToConsole { get; set; }
        public bool ColorizeLines { get; set; }

        private string FilePath { get; }

        public Logger(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            ColorizeLines = true;
            FilePath = Path.Combine(Defaults.LogDirectory, filePath);
        }

        public void Error(string message)
        {
            if (ColorizeLines)
                Console.ForegroundColor = ConsoleColor.Red;

            WriteLine($"[!][{DateTime.Now}] {message}");

            if(ColorizeLines)
                Console.ResetColor();
        }

        public void Info(string message, bool noNewLine = false)
        {
            if (ColorizeLines)
                Console.ForegroundColor = ConsoleColor.White;

            var msg = $"[i][{DateTime.Now}] {message}";

            if (noNewLine)
            {
                Write(msg);
            }
            else
            {
                WriteLine(msg);
            }

            if(ColorizeLines)
                Console.ResetColor();
        }

        public void Exception(Exception e)
        {
            if (ColorizeLines)
                Console.ForegroundColor = ConsoleColor.Red;

            WriteLine($"[e][{DateTime.Now}] {e.Message}");

            if(ColorizeLines)
                Console.ResetColor();

            WriteLine($"   Target site: {e.TargetSite}");
            WriteLine("   Stack trace:");
            foreach (var s in e.StackTrace.Split('\n'))
            {
                WriteLine($"      {s}");
            }
        }

        public void ExceptionSilent(Exception e)
        {
            WriteLineSilent($"[e][{DateTime.Now}] {e.Message}");
            WriteLineSilent($"   Target site: {e.TargetSite}");
            WriteLineSilent("   Stack trace:");
            foreach (var s in e.StackTrace.Split('\n'))
            {
                WriteLineSilent($"      {s}");
            }
        }

        public void WriteLine(string text)
        {
            using (var sw = new StreamWriter(FilePath, true))
            {
                sw.WriteLine(text);
            }

            if (WriteToConsole)
            {
                Console.WriteLine(text);
            }
        }

        private void WriteLineSilent(string text)
        {
            using (var sw = new StreamWriter(FilePath, true))
            {
                sw.WriteLine(text);
            }
        }

        private void Write(string text)
        {
            using (var sw = new StreamWriter(FilePath, true))
            {
                sw.Write(text);
            }

            if (WriteToConsole)
            {
                Console.Write(text);
            }
        }
    }
}
