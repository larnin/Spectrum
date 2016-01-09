using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Spectrum.Prism
{
    class Program
    {
        private static string _distanceAssemblyFilename;
        private static string _bootstrapAssemblyFilename;

        private static ModuleDefinition _distanceAssemblyDefinition;
        private static ModuleDefinition _bootstrapAssemblyDefinition;

        static void Main(string[] args)
        {
            WriteStartupHeader();

            if (!IsValidSyntax(args))
            {
                ColoredOutput.WriteInformation($"Usage: {GetExecutingFileName()} <TARGET DLL> <BOOTSTRAP DLL>");
                ErrorHandler.TerminateWithError("IOError: Invalid syntax provided.");
            }

            _distanceAssemblyFilename = args[0];
            _bootstrapAssemblyFilename = args[1];

            if (!DistanceFileExists())
                ErrorHandler.TerminateWithError("Specified TARGET DLL not found.");

            if (!BootstrapFileExists())
                ErrorHandler.TerminateWithError("Specified BOOTSTRAP DLL not found.");

            CreateBackup();
            ColoredOutput.WriteInformation("Now performing dispersion...");
            
            _distanceAssemblyDefinition = ModuleLoader.LoadDistanceModule(_distanceAssemblyFilename);
            _bootstrapAssemblyDefinition = ModuleLoader.LoadBootstrapModule(_bootstrapAssemblyFilename);

            InsertSpectrumInitCode();
            InsertSpectrumUpdateCode();

            ColoredOutput.WriteInformation("Writing modified file.");
            _distanceAssemblyDefinition.Write(_distanceAssemblyFilename);

            ColoredOutput.WriteSuccess("Dispersion complete. Spectrum should now be visible.");
        }

        private static void WriteStartupHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"Prism patcher for Spectrum {version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
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

        private static void InsertSpectrumInitCode()
        {
            try
            {
                var targetMethod = DispersionHelper.FindInitializationMethodDefinition(_distanceAssemblyDefinition);
                var ilProcessor = targetMethod.Body.GetILProcessor();

                var initMethodReference = DispersionHelper.ImportBootstrapMethodReference(_distanceAssemblyDefinition, _bootstrapAssemblyDefinition);
                var initializationInstruction = ilProcessor.Create(OpCodes.Call, initMethodReference);

                var lastAwakeInstruction = ilProcessor.Body.Instructions[ilProcessor.Body.Instructions.Count - 2];
                ilProcessor.InsertAfter(lastAwakeInstruction, initializationInstruction);

                ColoredOutput.WriteSuccess("Initialization code inserted.");
            }
            catch (Exception e)
            {
                ErrorHandler.TerminateWithError($"Couldn't insert initialization code. Exception details:\n{e}");
            }
        }

        private static void InsertSpectrumUpdateCode()
        {
            try
            {
                var targetMethod = DispersionHelper.FindUpdateMethodDefinition(_distanceAssemblyDefinition);
                var ilProcessor = targetMethod.Body.GetILProcessor();

                var updateMethodReference = DispersionHelper.ImportUpdateMethodReference(_distanceAssemblyDefinition, _bootstrapAssemblyDefinition);
                var updateInstruction = ilProcessor.Create(OpCodes.Call, updateMethodReference);

                var originalReturnInstruction = ilProcessor.Body.Instructions[ilProcessor.Body.Instructions.Count - 1];
                updateInstruction.Offset = originalReturnInstruction.Offset;

                ilProcessor.Replace(originalReturnInstruction, updateInstruction);

                var returnInstruction = ilProcessor.Create(OpCodes.Ret);
                ilProcessor.InsertAfter(updateInstruction, returnInstruction);
                
                ColoredOutput.WriteSuccess("Update code inserted.");
            }
            catch (Exception e)
            {
                ErrorHandler.TerminateWithError($"Couldn't insert update code. Exception details:\n{e}");
            }
        }
    }
}
