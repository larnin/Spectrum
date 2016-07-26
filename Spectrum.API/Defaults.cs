using System.IO;
using System.Reflection;

namespace Spectrum.API
{
    public class Defaults
    {
        private static string BasePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string ScriptDirectory => Path.Combine(BasePath, "Scripts");
        public static string OnDemandScriptDirectory => Path.Combine(ScriptDirectory, "OnDemand");
        public static string PluginDirectory => Path.Combine(BasePath, "Plugins");
        public static string PluginDataDirectory => Path.Combine(BasePath, "PluginData");
        public static string LogDirectory => Path.Combine(BasePath, "Logs");
        public static string SettingsDirectory => Path.Combine(BasePath, "Settings");
        public static string ResolverDirectory => Path.Combine(BasePath, "Dependencies");

        public const string HotkeyManagerLogFileName = "HotkeyManager.log";
        public const string LuaScriptSupportLogFileName = "LuaScriptSupport.log";
        public const string LuaExecutorLogFileName = "LuaExecutionEngine.log";
        public const string LuaLoaderLogFileName = "LuaLoader.log";
        public const string PluginLoaderLogFileName = "PluginLoader.log";
        public const string DependencyResolverLogFileName = "DependencyResolver.log";
    }
}
