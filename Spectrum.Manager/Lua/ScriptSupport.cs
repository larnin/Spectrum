using System;
using System.IO;
using NLua.Exceptions;
using Spectrum.API;
using Spectrum.API.Interfaces.Systems;
using Spectrum.Manager.Logging;

namespace Spectrum.Manager.Lua
{
    internal class ScriptSupport : IScriptSupport
    {
        public bool CanLoadScripts => Directory.Exists(Defaults.ScriptDirectory);
        public bool IsLuaInitialized { get; private set; }

        public NLua.Lua LuaState { get; private set; }

        public ScriptLoader Loader { get; private set; }
        public ScriptExecutor ExecutionEngine { get; private set; }

        public SubsystemLog Log { get; }

        public ScriptSupport()
        {
            Log = new SubsystemLog(Path.Combine(Defaults.LogDirectory, Defaults.LuaScriptSupportLogFileName));

            InitializeLua();
            LoadBasicPackages();
            AddCoreAliases();

            LoadScripts();
            InitializeExecutionEngine();
        }

        public void AddCLRPackage(string assemblyName, string wantedNamespace)
        {
            if (IsLuaInitialized)
            {
                try
                {
                    LuaState.DoString($"import('{assemblyName}', '{wantedNamespace}')");
                }
                catch (LuaException ex)
                {
                    Log.Error($"Couldn't import requested namespace '{wantedNamespace}' from '{assemblyName}'.");
                    Log.Exception(ex);
                }
                catch (Exception ex)
                {
                    Log.Error($"Unexpected error occured while importing namespace '{wantedNamespace}' from '{assemblyName}'.");
                    Log.Exception(ex);
                }
            }
            else
            {
                Log.Error("Tried to load a CLR package without Lua initialized.");
            }
        }

        public void AddNamespace(string wantedNamespace)
        {
            if (IsLuaInitialized)
            {
                try
                {
                    LuaState.DoString($"import('{wantedNamespace}')");
                }
                catch (LuaException ex)
                {
                    Log.Error($"Couldn't import requested namespace '{wantedNamespace}' from already loaded assemblies.");
                    Log.Exception(ex);
                }
                catch(Exception ex)
                {
                    Log.Error($"Unexpected error occured while importing namespace '{wantedNamespace}' from already loaded assemblies.");
                    Log.Exception(ex);
                }
            }
            else
            {
                Log.Error("Tried to add a namespace without Lua initialized.");
            }
        }

        public void AddGlobalAlias(object obj, string name)
        {
            Log.Info($"Added new global alias '{name}'.");
            LuaState[name] = obj;
        }

        public void ExecuteScript(string fileName)
        {
            ExecutionEngine.Execute(fileName);
        }

        private void InitializeLua()
        {
            try
            {
                Log.Info("Initializing Lua... ", true);
                LuaState = new NLua.Lua();
                LuaState.LoadCLRPackage();

                var version = (string)LuaState.DoString("return _VERSION")[0];
                Log.WriteLine(version);

                Log.Info("Adding global Lua namespace 'spectrum'...");
                LuaState.DoString("spectrum = { }");

                IsLuaInitialized = true;
            }
            catch (Exception ex)
            {
                Log.Error("An exception occured while initializing Lua. Check the log for details.");
                Log.Exception(ex);
            }
        }

        private void LoadBasicPackages()
        {
            AddCLRPackage("Spectrum.API.dll", "Spectrum.API");
            AddCLRPackage("Spectrum.Manager.dll", "Spectrum.Manager");
            AddCLRPackage("Assembly-CSharp.dll", "");
        }

        private void AddCoreAliases()
        {
            LuaState.DoString("import_namespace = luanet.namespace");
            LuaState.DoString("load_assembly = luanet.load_assembly");
            LuaState.DoString("import_type = luanet.import_type");
            LuaState.DoString("each = luanet.each");
            LuaState.DoString("enum = luanet.enum");
        }

        private void LoadScripts()
        {
            if (CanLoadScripts)
            {
                Loader = new ScriptLoader(Defaults.ScriptDirectory, Defaults.OnDemandScriptDirectory);
                Loader.LoadAll();

                IsLuaInitialized = true;
            }
            else
            {
                Log.Error($"Can't load or execute scripts. Directory '{Defaults.ScriptDirectory}' does not exist.");
            }
        }

        private void InitializeExecutionEngine()
        {
            if (IsLuaInitialized)
            {
                try
                {
                    ExecutionEngine = new ScriptExecutor(LuaState, Loader);
                    ExecutionEngine.ExecuteAllStartupScripts();
                }
                catch(Exception ex)
                {
                    IsLuaInitialized = false;
                    Log.Exception(ex);
                }
            }
            else
            {
                Log.Error("Tried to execute script without Lua initialized.");
            }
        }
    }
}
