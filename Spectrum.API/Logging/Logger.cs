using System;
using System.IO;

namespace Spectrum.API.Logging
{
    public class Logger
    {
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
        public ConsoleColor WarningColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor InfoColor { get; set; } = ConsoleColor.White;
        public ConsoleColor ExceptionColor { get; set; } = ConsoleColor.Magenta;

        public bool WriteToConsole { get; set; }
        public bool ColorizeLines { get; set; }

        private string FilePath { get; }

        public Logger(string fileName)
        {
            ColorizeLines = true;
            FilePath = Path.Combine(Defaults.LogDirectory, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        public void Error(string message)
        {
            if (ColorizeLines)
                Console.ForegroundColor = ErrorColor;

            WriteLine($"[!][{DateTime.Now}] {message}");

            if (ColorizeLines)
                Console.ResetColor();
        }

        public void Warning(string message)
        {
            if (ColorizeLines)
                Console.ForegroundColor = WarningColor;

            WriteLine($"[*][{DateTime.Now}] {message}");

            if (ColorizeLines)
                Console.ResetColor();
        }

        public void Info(string message, bool noNewLine = false)
        {
            if (ColorizeLines)
                Console.ForegroundColor = InfoColor;

            var msg = $"[i][{DateTime.Now}] {message}";

            if (noNewLine)
            {
                Write(msg);
            }
            else
            {
                WriteLine(msg);
            }

            if (ColorizeLines)
                Console.ResetColor();
        }

        public void Exception(Exception e)
        {
            if (ColorizeLines)
                Console.ForegroundColor = ExceptionColor;

            WriteLine($"[e][{DateTime.Now}] {e.Message}");

            if (ColorizeLines)
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
