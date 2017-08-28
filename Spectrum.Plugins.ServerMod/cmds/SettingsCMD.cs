using Spectrum.API;
using Spectrum.Plugins.ServerMod.CmdSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.cmds
{
    class SettingsCMD : cmd
    {
        public override string name { get { return "settings"; } }
        public override PermType perm { get { return PermType.HOST; } }
        public override bool canUseAsClient { get { return true; } }

        public override CmdSetting[] settings { get; } =
        {
            new CmdSettingRecentVersion()
        };

        private const string settingRegex = @"^\s*(\w+) (.*)[\r\n]*$";
        private const string baseRegex = @"^\s*(\w+)[\r\n]*$";

        public override void help(ClientPlayerInfo p)
        {
            Utilities.sendMessage($"{Utilities.formatCmd("!settings reload")}: reload the settings from the file.");
            Utilities.sendMessage($"{Utilities.formatCmd("!settings summary")}: view the value of all settings.");
            Utilities.sendMessage($"{Utilities.formatCmd("!settings default <setting>")}: reset <setting> to its default value");
            Utilities.sendMessage($"{Utilities.formatCmd("!settings help <setting>")}: view a more detailed help message for <setting>");
            foreach (cmd Command in cmd.all.list())
            {
                string txt = "";
                foreach (CmdSetting Setting in Command.settings)
                {
                    if (Setting.SettingsId != "")
                       txt += $"\n {Utilities.formatCmd($"!settings {Setting.SettingsId} {Setting.UsageParameters}")}: {Setting.HelpShort}";
                }
                if (txt != "")
                {
                    Utilities.sendMessage($"[b][D00000]!{Command.name} Settings[-][/b]{txt}");
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
                string setting = msgMatch.Groups[1].Value.ToLower();
                if (setting == "reload")
                {
                    reload(p);
                    return;
                }
                else if (setting == "writemarkdown")
                {  // command used to generate markdown help for settings. not shown to user.
                    string style = msgMatch.Groups[2].Value.ToLower();
                    if (style == "command" || style == "both")
                    {
                        string txt = "";
                        foreach (cmd Command in cmd.all.list())
                        {
                            string txt2 = "";
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.SettingsId != "")
                                    txt2 += $"  * `!settings {Setting.SettingsId} {Setting.UsageParameters}`  \n{Setting.HelpMarkdown}  \nDefault: {Setting.Default}  \n";
                            }
                            if (txt2 != "")
                                txt += $"* `!{Command.name}` Settings\n" + txt2;
                        }
                        string FilePath = Path.Combine(Defaults.SettingsDirectory, "servermod-settings.command.md");
                        using (var sw = new StreamWriter(FilePath, false))
                        {
                            sw.Write(txt);
                        }
                    }

                    if (style == "file" || style == "both")
                    {
                        string txt = "";
                        foreach (cmd Command in cmd.all.list())
                        {
                            string txt2 = "";
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.FileId != "")
                                 txt2 += $"* `\"{Setting.FileId}\" :  {Setting.UsageParameters},`  \n{Setting.HelpMarkdown}  \nDefault: {Setting.Default}  \n";
                            }
                            if (txt2 != "")
                                txt += txt2;
                        }
                        string FilePath = Path.Combine(Defaults.SettingsDirectory, "servermod-settings.file.md");
                        using (var sw = new StreamWriter(FilePath, false))
                        {
                            sw.Write(txt);
                        }
                    }

                    if (style != "file" && style != "command" && style != "both")
                        Console.WriteLine("Unknown style type. Styles: command, file, both");
                    return;
                }
                else if (setting == "help")
                {
                    string settingHelp = msgMatch.Groups[2].Value.ToLower();
                    if (settingHelp.Length == 0)
                    {
                        help(p);
                        return;
                    }
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
                    Utilities.sendMessage($"Could not find setting by the name of `{msgMatch.Groups[2].Value}`");
                }
                else if (setting == "default")
                {
                    string settingHelp = msgMatch.Groups[2].Value.ToLower();
                    if (settingHelp.Length == 0)
                    {
                        help(p);
                        return;
                    }
                    foreach (cmd Command in cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == settingHelp.ToLower())
                            {
                                Setting.Value = Setting.Default;
                                Utilities.sendMessage($"Setting {Setting.SettingsId} reset to default:");
                                Utilities.sendMessage($"{Setting.Default}");
                                Entry.save();
                                return;
                            }
                        }
                    }
                    Utilities.sendMessage($"Could not find setting by the name of `{msgMatch.Groups[2].Value}`");
                }
                else
                {
                    foreach (cmd Command in cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == setting.ToLower())
                            {
                                UpdateResult result = Setting.UpdateFromString(msgMatch.Groups[2].Value);
                                if (!result.Valid)
                                    Utilities.sendMessage($"Failed to set setting: {result.Message}");
                                else
                                {
                                    Utilities.sendMessage($"Set {Setting.SettingsId} successfully.");
                                    if (result.Message != "")
                                        Utilities.sendMessage(result.Message);
                                }
                                Setting.Value = result.NewValue;
                                Entry.save();
                                return;
                            }
                        }
                    }
                    Utilities.sendMessage($"Could not find setting by the name of `{msgMatch.Groups[1].Value}`");
                    return;
                }

            }
            else
            {
                Match baseMatch = Regex.Match(message, baseRegex);
                if (baseMatch.Success)
                {
                    string setting = baseMatch.Groups[1].Value.ToLower();
                    if (setting == "reload")
                    {
                        reload(p);
                        return;
                    }
                    else if (setting == "summary")
                    {
                        foreach (cmd Command in cmd.all.list())
                        {
                            string txt = "";
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.SettingsId != "")
                                    txt += $"\n {Utilities.formatCmd($"{Setting.SettingsId}")}: {Setting.Value}";
                            }
                            if (txt != "")
                            {
                                Utilities.sendMessage($"[b][D00000]!{Command.name} Settings[-][/b]{txt}");
                            }
                        }
                        return;
                    }
                    else
                    {
                        foreach (cmd Command in cmd.all.list())
                        {
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.SettingsId.ToLower() == setting.ToLower())
                                {
                                    Utilities.sendMessage($"Value of {Setting.SettingsId}: {Setting.Value}");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            help(p);
        }

        void reload(ClientPlayerInfo p)
        {
            Entry.reload();
            Utilities.sendMessage("Settings reloaded from file!");
        }

        public void showNewSettings()
        {
            var currentVersionMatch = Regex.Match(Entry.PluginVersion, UpdateCMD.updateCheckLocalRegex);
            if (!currentVersionMatch.Success)
            {
                Utilities.sendMessage("Warning: Could not match current plugin version. Cannot show new settings or check for updates.");
                return;
            }
            var currentVersion = currentVersionMatch.Groups[1].Value;
            if (currentVersion == (string)(getSetting("recentVersion").Value))
                return;
            getSetting("recentVersion").Value = currentVersion;
            Entry.save();
            Utilities.sendMessage($"[b][D00000]New Settings Defaults for {Entry.PluginVersion}[-][/b]");
            var count = 0;
            foreach (cmd Command in cmd.all.list())
            {
                string txt = "";
                foreach (CmdSetting Setting in Command.settings)
                {
                    if (Setting.SettingsId != "" && Setting.UpdatedOnVersion == currentVersion && !Setting.Value.Equals(Setting.Default))
                        txt += $"\n {Utilities.formatCmd($"{Setting.SettingsId}")}:\n  [FFFFFF]Default:[-] {Setting.Default}\n  [FFFFFF]  Yours:[-] {Setting.Value}";
                }
                if (txt != "")
                {
                    count++;
                    Utilities.sendMessage($"[D00000]!{Command.name} Settings[-]{txt}");
                }
            }
            if (count == 0)
                Utilities.sendMessage("None");
            else
                Utilities.sendMessage($"[FFFFFF]Use {Utilities.formatCmd("!settings default <setting>")} to reset a setting to its default.");
        }
    }
    class CmdSettingRecentVersion : CmdSettingString
    {
        public override string FileId { get; } = "recentVersion";
        public override string SettingsId { get; } = "";

        public override string DisplayName { get; } = "Recent ServerMod Version";
        public override string HelpShort { get; } = "Stores the most recent version of ServerMod to detect when ServerMod has been updated.";
        public override string HelpLong { get { return HelpShort; } }

        public override object Default { get { return ""; } }
        public override string UpdatedOnVersion { get; } = "C.7.4.0";
    }
}
