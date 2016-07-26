namespace Spectrum.API.Interfaces.Systems
{
    public interface IManager
    {
        IScriptSupport Scripts { get; }
        IHotkeyManager Hotkeys { get; }
    }
}
