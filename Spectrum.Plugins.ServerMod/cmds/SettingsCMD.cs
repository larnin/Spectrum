using Spectrum.Plugins.ServerMod.CmdSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class SettingsCMD : cmd
    {
        public override string name { get { return "settings"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return false; } }

        private const string settingRegex = @"^\s*(\w+) (.*)[\r\n]*$";

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage($"{Utilities.formatCmd("!settings reload")}: reload the settings from the file.");
            Utilities.sendMessage($"{Utilities.formatCmd("!settings help <setting>")}: view a more detailed help message for <setting>");
            foreach (cmd Command in cmd.all.list())
            {
                Utilities.sendMessage($"[b][D00000]!{Command.name} Settings[-][/b]");
                foreach (CmdSetting Setting in Command.settings)
                {
                    Utilities.sendMessage($" {Utilities.formatCmd($"!settings {Setting.SettingsId} {Setting.UsageParameters}")}: {Setting.HelpShort}");
                }
            }
        }

        public override void use(ClientPlayerInfo p, string message)
        {
            if(message == "")
            {
                help(p);
                return;
            }


            Match msgMatch = Regex.Match(message, settingRegex);
            if (msgMatch.Success)
            {
                string setting = msgMatch.Groups[0].Value.ToLower();
                if (setting == "reload")
                    reload(p);
                else if (setting == "help")
                {
                    string settingHelp = msgMatch.Groups[1].Value.ToLower();
                    foreach (cmd Command in cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == settingHelp.ToLower())
                            {
                                Utilities.sendMessage($"{Utilities.formatCmd($"!settings {Setting.SettingsId} {Setting.UsageParameters}")}");
                                Utilities.sendMessage(Setting.HelpLong);
                                return;
                            }
                        }
                    }
                    Utilities.sendMessage($"Could not find setting by the name of `{msgMatch.Groups[1].Value}`");
                }
                else
                {
                    foreach (cmd Command in cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == setting.ToLower())
                            {
                                UpdateResult result = Setting.UpdateFromString(msgMatch.Groups[1].Value);
                                if (!result.Valid)
                                    Utilities.sendMessage($"Failed to set setting: {result.Message}");
                                else if (result.Message != "")
                                    Utilities.sendMessage(result.Message);
                                Setting.Value = result.NewValue;
                                Entry.save();
                                return;
                            }
                        }
                    }
                    Utilities.sendMessage($"Could not find setting by the name of `{msgMatch.Groups[0].Value}`");
                }

            }
            else
                help(p);
        }

        void reload(ClientPlayerInfo p)
        {
            Entry.reload();
            Utilities.sendMessage("Settings reloaded from file!");
        }
    }
}
