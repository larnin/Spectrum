namespace Spectrum.API.Interfaces.Systems
{
    public interface IManager
    {
        ILoader LuaLoader { get; }
        IExecutor LuaExecutor { get; }
        IHotkeyManager Hotkeys { get; }
    }
}
