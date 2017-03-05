using Spectrum.API;
using Spectrum.API.Game;
using Spectrum.API.Game.Vehicle;
using Spectrum.API.Game.EventArgs.Vehicle;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using Spectrum.API.Configuration;

namespace Spectrum.Plugins.TutorialPlugin
{
    public class Entry : IPlugin
    {
        public string FriendlyName => "Reset = Restart";
        public string Author => "Jonathan Vollebregt";
        public string Contact => "jnvsor@gmail.com";
        public APILevel CompatibleAPILevel => APILevel.UltraViolet;

        private Settings Settings = new Settings(typeof(Entry));
        private bool Enabled = false;

        public void Initialize(IManager manager)
        {
            ValidateSettings();

            Enabled = (bool) Settings["OnByDefault"];

            manager.Hotkeys.Bind(Settings["ToggleResetRestartHotkey"] as string, Toggle, true);
            LocalVehicle.Exploded += Explode;
        }

        public void Shutdown()
        {
        }

        private void Toggle()
        {
            Enabled = !Enabled;
        }

        private void Explode(object sender, DestroyedEventArgs args)
        {
            if (Enabled && args.Cause == DestructionCause.SelfTermination) {
                Game.RestartLevel();
            }
        }

        private void ValidateSettings()
        {
            if (!Settings.ContainsKey("ToggleResetRestartHotkey"))
            {
                Settings["ToggleResetRestartHotkey"] = "LeftControl+T";
            }
            if (!Settings.ContainsKey("OnByDefault"))
            {
                Settings["OnByDefault"] = false;
            }

            Settings.Save();
        }
    }
}
