using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectrum.API;
using Spectrum.API.Interfaces.Systems;
using Spectrum.Manager.Logging;

namespace Spectrum.Manager.Lua
{
    public class Loader : ILoader
    {
        private SubsystemLog Log { get; }
        public string ScriptFolder { get; }
        public string OnDemandScriptFolder { get; }

        public List<string> ScriptPaths { get; private set; }
        public List<string> OnDemandScriptPaths { get; private set; } 

        public Loader(string scriptFolder, string onDemandScriptFolder)
        {
            ScriptFolder = scriptFolder;
            OnDemandScriptFolder = onDemandScriptFolder;

            Log = new SubsystemLog(Path.Combine(Defaults.LogDirectory, Defaults.LuaLoaderLogFileName), true);
            Log.Info("Lua loader starting up...");
        }

        public void LoadAll()
        {
            ScriptPaths = Directory.GetFiles(ScriptFolder, "*.lua").ToList();
            Log.Info($"Loaded {ScriptPaths.Count} startup scripts.");

            OnDemandScriptPaths = Directory.GetFiles(OnDemandScriptFolder, "*.lua").ToList();
            Log.Info($"Loaded {OnDemandScriptPaths.Count} on-demand scripts.");
        }
    }
}
