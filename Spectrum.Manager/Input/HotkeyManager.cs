using System;
using System.Collections.Generic;
using System.IO;
using Spectrum.API;
using Spectrum.API.Input;
using Spectrum.API.Interfaces.Systems;
using Spectrum.Manager.Logging;

namespace Spectrum.Manager.Input
{
    public class HotkeyManager : IHotkeyManager
    {
        private Dictionary<Hotkey, Action> ActionHotkeys { get; }
        private Dictionary<Hotkey, string> ScriptHotkeys { get; }

        private SubsystemLog Log { get; }
        private Manager Manager { get; }

        public HotkeyManager(Manager manager)
        {
            ActionHotkeys = new Dictionary<Hotkey, Action>();
            ScriptHotkeys = new Dictionary<Hotkey, string>();

            Log = new SubsystemLog(Path.Combine(Defaults.LogDirectory, Defaults.HotkeyManagerLogFileName), true);
            Manager = manager;
        }

        public void Bind(Hotkey hotkey, Action action)
        {
            if (Exists(hotkey))
            {
                WriteExistingHotkeyInfo(hotkey);
                return;
            }
            ActionHotkeys.Add(hotkey, action);
            Log.Info($"Bound '{hotkey}' to a plugin-defined action.");
        }

        public void Bind(string hotkeyString, Action action)
        {
            Bind(new Hotkey(hotkeyString), action);
        }

        public void Bind(Hotkey hotkey, string scriptFileName)
        {
            if (Exists(hotkey))
            {
                WriteExistingHotkeyInfo(hotkey);
                return;
            }
            ScriptHotkeys.Add(hotkey, scriptFileName);
            Log.Info($"Bound '{hotkey}' to the on-demand script '{scriptFileName}'.");
        }

        public void Bind(string hotkeyString, string scriptFileName)
        {
            Bind(new Hotkey(hotkeyString), scriptFileName);
        }

        public void Unbind(string hotkeyString)
        {
            foreach (var hotkey in ScriptHotkeys)
            {
                if (hotkey.ToString() == hotkeyString)
                {
                    ScriptHotkeys.Remove(hotkey.Key);
                }
            }

            foreach (var hotkey in ActionHotkeys)
            {
                if (hotkey.ToString() == hotkeyString)
                {
                    ActionHotkeys.Remove(hotkey.Key);
                }
            }
        }

        public void UnbindAll()
        {
            ScriptHotkeys.Clear();
            ActionHotkeys.Clear();
        }

        public bool Exists(Hotkey hotkey)
        {
            return ScriptHotkeys.ContainsKey(hotkey) || ActionHotkeys.ContainsKey(hotkey);
        }

        public bool Exists(string hotkeyString)
        {
            foreach (var hotkey in ScriptHotkeys)
            {
                if (hotkey.ToString() == hotkeyString)
                    return true;
            }

            foreach (var hotkey in ActionHotkeys)
            {
                if (hotkey.ToString() == hotkeyString)
                    return true;
            }

            return false;
        }

        public bool IsScriptHotkey(Hotkey hotkey)
        {
            return ScriptHotkeys.ContainsKey(hotkey);
        }

        public bool IsActionHotkey(Hotkey hotkey)
        {
            return ActionHotkeys.ContainsKey(hotkey);
        }

        internal void Update()
        {
            if (ScriptHotkeys.Count > 0)
            {
                foreach (var hotkey in ScriptHotkeys)
                {
                    if (hotkey.Key.IsPressed)
                    {
                        Manager.LuaExecutor.Execute(hotkey.Value);
                    }
                }
            }

            if (ActionHotkeys.Count > 0)
            {
                foreach (var hotkey in ActionHotkeys)
                {
                    if (hotkey.Key.IsPressed)
                    {
                        hotkey.Value.Invoke();
                    }
                }
            }
        }

        private void WriteExistingHotkeyInfo(Hotkey hotkey)
        {
            if (IsActionHotkey(hotkey))
            {
                Log.Error($"The hotkey '{hotkey}' is already bound to another action.");
            }

            if (IsScriptHotkey(hotkey))
            {
                Log.Error($"The hotkey '{hotkey}' is already bound to the script '{ScriptHotkeys[hotkey]}'.");
            }
        }
    }
}
