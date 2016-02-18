using System;
using Spectrum.API.Input;

namespace Spectrum.API.Interfaces.Systems
{
    public interface IManager
    {
        ILoader LuaLoader { get; }
        IExecutor LuaExecutor { get; }

        void AddHotkey(Hotkey hotkey, Action action);
    }
}
