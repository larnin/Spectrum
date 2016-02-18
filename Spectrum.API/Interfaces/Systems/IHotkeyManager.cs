using System;
using Spectrum.API.Input;

namespace Spectrum.API.Interfaces.Systems
{
    public interface IHotkeyManager
    {
        void Bind(Hotkey hotkey, Action action);
        void Bind(string hotkey, Action action);
        void Bind(Hotkey hotkey, string scriptFileName);
        void Bind(string hotkey, string scriptFileName);

        void Unbind(string hotkeyString);
        void UnbindAll();
    }
}
