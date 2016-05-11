using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Spectrum.Prism.IO;
using Spectrum.Prism.Patches;
using Spectrum.Prism.Runtime;

namespace Spectrum.Prism
{
    internal class Program
    {
        private static string _distanceAssemblyFilename;
        private static string _bootstrapAssemblyFilename;

        private static ModuleDefinition _distanceAssemblyDefinition;
        private static ModuleDefinition _bootstrapAssemblyDefinition;

        private static Patcher _patcher;

        internal static void Main(string[] args)
        {
            WriteStartupHeader();

            if (!IsValidSyntax(args))
            {
                ColoredOutput.WriteInformation($"Usage: {GetExecutingFileName()} <TARGET DLL> <BOOTSTRAP DLL>");
                ErrorHandler.TerminateWithError("Invalid syntax provided.");
            }

            _distanceAssemblyFilename = args[0];
            _bootstrapAssemblyFilename = args[1];

            if (!DistanceFileExists())
                ErrorHandler.TerminateWithError("Specified TARGET DLL not found.");

            if (!BootstrapFileExists())
                ErrorHandler.TerminateWithError("Specified BOOTSTRAP DLL not found.");

            CreateBackup();
            PreparePatches();
            RunPatches();

            ModuleWriter.SavePatchedFile(_distanceAssemblyDefinition, _distanceAssemblyFilename);
            ColoredOutput.WriteSuccess("Patch process completed.");
        }

        private static void WriteStartupHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"Prism patcher for Spectrum. Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
            Console.WriteLine("------------------------------------------");
            Console.ResetColor();
        }

        private static bool IsValidSyntax(ICollection<string> args)
        {
            return args.Count == 2;
        }

        private static string GetExecutingFileName()
        {
            return Path.GetFileName(Assembly.GetExecutingAssembly().Location);
        }

        private static bool DistanceFileExists()
        {
            return File.Exists(_distanceAssemblyFilename);
        }

        private static bool BootstrapFileExists()
        {
            return File.Exists(_bootstrapAssemblyFilename);
        }

        private static void CreateBackup()
        {
            if (!File.Exists($"{_distanceAssemblyFilename}.backup"))
            {
                ColoredOutput.WriteInformation("Performing a backup...");
                File.Copy($"{_distanceAssemblyFilename}", $"{_distanceAssemblyFilename}.backup");
            }
        }

        private static void PreparePatches()
        {
            ColoredOutput.WriteInformation("Preparing patches...");

            _distanceAssemblyDefinition = ModuleLoader.LoadDistanceModule(_distanceAssemblyFilename);
            _bootstrapAssemblyDefinition = ModuleLoader.LoadBootstrapModule(_bootstrapAssemblyFilename);
            _patcher = new Patcher(_bootstrapAssemblyDefinition, _distanceAssemblyDefinition);

            _patcher.AddPatch(new SpectrumInitCodePatch());
            _patcher.AddPatch(new SpectrumUpdateCodePatch());
        }

        private static void RunPatches()
        {
            ColoredOutput.WriteInformation("Running patches...");
            _patcher.RunAll();
        }
    }
}
