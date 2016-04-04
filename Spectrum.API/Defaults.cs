using System.IO;
using System.Reflection;

namespace Spectrum.API
{
    public class Defaults
    {
        private static string BasePath => Assembly.GetExecutingAssembly().Location;

        public string ScriptDirectory => Path.Combine(BasePath, "Scripts");
        public string OnDemandScriptDirectory => Path.Combine(ScriptDirectory, "OnDemand");
        public string PluginDirectory => Path.Combine(BasePath, "Plugins");
        public string PluginDataDirectory => Path.Combine(BasePath, "PluginData");
        public string LogDirectory = Path.Combine(BasePath, "Logs");
        public string SettingsDirectory => Path.Combine(BasePath, "Settings");

        public const string HotkeyManagerLogFileName = "HotkeyManager.log";
        public const string LuaExecutorLogFileName = "LuaExecutor.log";
        public const string LuaLoaderLogFileName = "LuaLoader.log";
        public const string PluginLoaderLogFileName = "PluginLoader.log";
    }
}
