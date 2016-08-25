using System;
using System.Collections.Generic;
using Spectrum.API;
using Spectrum.API.Input;
using Spectrum.API.Interfaces.Systems;
using Spectrum.API.Logging;

namespace Spectrum.Manager.Input
{
    public class HotkeyManager : IHotkeyManager
    {
        private Dictionary<Hotkey, Action> ActionHotkeys { get; }

        private Logger Log { get; }

        public HotkeyManager()
        {
            ActionHotkeys = new Dictionary<Hotkey, Action>();

            Log = new Logger(Defaults.HotkeyManagerLogFileName)
            {
                WriteToConsole = Global.Settings.GetSection("Output").GetValue<bool>("LogToConsole")
            };
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

        public void Bind(string hotkeyString, Action action, bool isOneTime)
        {
            Bind(new Hotkey(hotkeyString, isOneTime), action);
        }

        public void Bind(Hotkey hotkey, string scriptFileName)
        {
            if (Exists(hotkey))
            {
                WriteExistingHotkeyInfo(hotkey);
                return;
            }
            Log.Info($"Bound '{hotkey}' to the on-demand script '{scriptFileName}'.");
        }

        public void Bind(string hotkeyString, string scriptFileName)
        {
            Bind(new Hotkey(hotkeyString), scriptFileName);
        }

        public void Bind(string hotkeyString, string scriptFileName, bool isOneTime)
        {
            Bind(new Hotkey(hotkeyString, isOneTime), scriptFileName);
        }

        public void Unbind(string hotkeyString)
        {
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
            ActionHotkeys.Clear();
        }

        public bool Exists(Hotkey hotkey)
        {
            return ActionHotkeys.ContainsKey(hotkey);
        }

        public bool Exists(string hotkeyString)
        {
            foreach (var hotkey in ActionHotkeys)
            {
                if (hotkey.ToString() == hotkeyString)
                    return true;
            }

            return false;
        }

        public bool IsActionHotkey(Hotkey hotkey)
        {
            return ActionHotkeys.ContainsKey(hotkey);
        }

        internal void Update()
        {
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
        }
    }
}
