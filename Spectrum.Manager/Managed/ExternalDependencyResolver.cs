using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Spectrum.API;
using Spectrum.API.Logging;

namespace Spectrum.Manager.Managed
{
    internal class ExternalDependencyResolver
    {
        private Logger Log { get; }

        internal ExternalDependencyResolver()
        {
            Log = new Logger(Defaults.DependencyResolverLogFileName)
            {
                WriteToConsole = Global.Settings.GetValue<bool>("LogToConsole")
            };
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Log.Info("External dependency resolver initialized.");
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var dependencyAssembliesPaths = Directory.GetFiles(Defaults.ResolverDirectory, "*.dll").ToList();
            dependencyAssembliesPaths.AddRange(Directory.GetFiles(".", "*.dll"));
            dependencyAssembliesPaths.AddRange(Directory.GetFiles("../Managed", "*.dll"));

            Log.Info($"Trying to resolve dependency assembly '{args.Name}'...");
            try
            {
                foreach (var path in dependencyAssembliesPaths)
                {
                    var assemblyName = AssemblyName.GetAssemblyName(path);
                    if (assemblyName.FullName == args.Name)
                    {
                        Log.Info($"Loaded '{args.Name}'.");
                        return Assembly.LoadFrom(path);
                    }
                }
            }
            catch
            {
                Log.Error($"Dependency assembly {args.Name} is missing or corrupted.");
                return null;
            }
            Log.Error($"Dependency assembly {args.Name} is missing or corrupted.");
            return null;
        }
    }
}
