using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spectrum.API;
using Spectrum.Manager.Logging;

namespace Spectrum.Manager.Lua
{
    public class Loader
    {
        private SubsystemLog Log { get; }
        public string ScriptFolder { get; }
        public List<string> ScriptPaths { get; private set; }

        public Loader(string scriptFolder)
        {
            ScriptFolder = scriptFolder;
            ScriptPaths = new List<string>();

            Log = new SubsystemLog(Path.Combine(Defaults.LogDirectory, Defaults.LuaLoaderLogFileName), true);
            Log.Info("Lua loader starting up...");
        }

        public void LoadScripts()
        {
            ScriptPaths = Directory.GetFiles(ScriptFolder, "*.lua").ToList();
            Log.Info($"Loaded {ScriptPaths.Count} scripts.");
        }
    }
}
