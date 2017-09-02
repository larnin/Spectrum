using Spectrum.API;
using Spectrum.Plugins.ServerMod.CmdSettings;
using Spectrum.Plugins.ServerMod.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spectrum.Plugins.ServerMod.Cmds
{
    class SettingsCmd : Cmd
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
            MessageUtilities.sendMessage($"{GeneralUtilities.formatCmd("!settings reload")}: reload the settings from the file.");
            MessageUtilities.sendMessage($"{GeneralUtilities.formatCmd("!settings summary")}: view the value of all settings.");
            MessageUtilities.sendMessage($"{GeneralUtilities.formatCmd("!settings defaults")}: view the default of all settings.");
            MessageUtilities.sendMessage($"{GeneralUtilities.formatCmd("!settings reset <setting>")}: reset <setting> to its default value");
            MessageUtilities.sendMessage($"{GeneralUtilities.formatCmd("!settings default <setting>")}: view default for <setting>");
            MessageUtilities.sendMessage($"{GeneralUtilities.formatCmd("!settings help <setting>")}: view a more detailed help message for <setting>");
            foreach (Cmd Command in Cmd.all.list())
            {
                string txt = "";
                foreach (CmdSetting Setting in Command.settings)
                {
                    if (Setting.SettingsId != "")
                       txt += $"\n {GeneralUtilities.formatCmd($"!settings {Setting.SettingsId} {Setting.UsageParameters}")}: {Setting.HelpShort}";
                }
                if (txt != "")
                {
                    MessageUtilities.sendMessage($"[b][D00000]!{Command.name} Settings[-][/b]{txt}");
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
                        foreach (Cmd Command in Cmd.all.list())
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
                        foreach (Cmd Command in Cmd.all.list())
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
                    foreach (Cmd Command in Cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == settingHelp.ToLower())
                            {
                                MessageUtilities.sendMessage($"{GeneralUtilities.formatCmd($"!settings {Setting.SettingsId} {Setting.UsageParameters}")}");
                                MessageUtilities.sendMessage(Setting.HelpLong);
                                return;
                            }
                        }
                    }
                    MessageUtilities.sendMessage($"Could not find setting by the name of `{msgMatch.Groups[2].Value}`");
                }
                else if (setting == "reset")
                {
                    string settingHelp = msgMatch.Groups[2].Value.ToLower();
                    if (settingHelp.Length == 0)
                    {
                        help(p);
                        return;
                    }
                    foreach (Cmd Command in Cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == settingHelp.ToLower())
                            {
                                Setting.Value = Setting.Default;
                                MessageUtilities.sendMessage($"Setting {Setting.SettingsId} reset to default:");
                                MessageUtilities.sendMessage($"{Setting.Default}");
                                Entry.save();
                                return;
                            }
                        }
                    }
                    MessageUtilities.sendMessage($"Could not find setting by the name of `{msgMatch.Groups[2].Value}`");
                }
                else if (setting == "default")
                {
                    string settingHelp = msgMatch.Groups[2].Value.ToLower();
                    if (settingHelp.Length == 0)
                    {
                        help(p);
                        return;
                    }
                    foreach (Cmd Command in Cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == settingHelp.ToLower())
                            {
                                MessageUtilities.sendMessage($"{Setting.SettingsId} default:");
                                MessageUtilities.sendMessage($"{Setting.Default}");
                                return;
                            }
                        }
                    }
                    MessageUtilities.sendMessage($"Could not find setting by the name of `{msgMatch.Groups[2].Value}`");
                }
                else
                {
                    foreach (Cmd Command in Cmd.all.list())
                    {
                        foreach (CmdSetting Setting in Command.settings)
                        {
                            if (Setting.SettingsId.ToLower() == setting.ToLower())
                            {
                                UpdateResult result = Setting.UpdateFromString(msgMatch.Groups[2].Value);
                                if (!result.Valid)
                                    MessageUtilities.sendMessage($"Failed to set setting: {result.Message}");
                                else
                                {
                                    MessageUtilities.sendMessage($"Set {Setting.SettingsId} successfully.");
                                    if (result.Message != "")
                                        MessageUtilities.sendMessage(result.Message);
                                }
                                Setting.Value = result.NewValue;
                                Entry.save();
                                return;
                            }
                        }
                    }
                    MessageUtilities.sendMessage($"Could not find setting by the name of `{msgMatch.Groups[1].Value}`");
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
                        foreach (Cmd Command in Cmd.all.list())
                        {
                            string txt = "";
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.SettingsId != "")
                                    txt += $"\n {GeneralUtilities.formatCmd($"{Setting.SettingsId}")}: {Setting.Value}";
                            }
                            if (txt != "")
                            {
                                MessageUtilities.sendMessage($"[b][D00000]!{Command.name} Settings[-][/b]{txt}");
                            }
                        }
                        return;
                    }
                    else if (setting == "defaults")
                    {
                        foreach (Cmd Command in Cmd.all.list())
                        {
                            string txt = "";
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.SettingsId != "")
                                    txt += $"\n {GeneralUtilities.formatCmd($"{Setting.SettingsId}")}: {Setting.Default}";
                            }
                            if (txt != "")
                            {
                                MessageUtilities.sendMessage($"[b][D00000]!{Command.name} Default Settings[-][/b]{txt}");
                            }
                        }
                        return;
                    }
                    else
                    {
                        foreach (Cmd Command in Cmd.all.list())
                        {
                            foreach (CmdSetting Setting in Command.settings)
                            {
                                if (Setting.SettingsId.ToLower() == setting.ToLower())
                                {
                                    MessageUtilities.sendMessage($"Value of {Setting.SettingsId}: {Setting.Value}");
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
            MessageUtilities.sendMessage("Settings reloaded from file!");
        }

        public void showNewSettings()
        {
            var currentVersionMatch = Regex.Match(Entry.PluginVersion, UpdateCmd.updateCheckLocalRegex);
            if (!currentVersionMatch.Success)
            {
                MessageUtilities.sendMessage("Warning: Could not match current plugin version. Cannot show new settings or check for updates.");
                return;
            }
            var currentVersion = currentVersionMatch.Groups[1].Value;
            if (currentVersion == (string)(getSetting("recentVersion").Value))
                return;
            getSetting("recentVersion").Value = currentVersion;
            Entry.save();
            MessageUtilities.sendMessage($"[b][D00000]New Settings Defaults for {Entry.PluginVersion}[-][/b]");
            var count = 0;
            foreach (Cmd Command in Cmd.all.list())
            {
                string txt = "";
                foreach (CmdSetting Setting in Command.settings)
                {
                    if (Setting.SettingsId != "" && Setting.UpdatedOnVersion == currentVersion && !Setting.Value.Equals(Setting.Default))
                        txt += $"\n {GeneralUtilities.formatCmd($"{Setting.SettingsId}")}:\n  [FFFFFF]Default:[-] {Setting.Default}\n  [FFFFFF]  Yours:[-] {Setting.Value}";
                }
                if (txt != "")
                {
                    count++;
                    MessageUtilities.sendMessage($"[D00000]!{Command.name} Settings[-]{txt}");
                }
            }
            if (count == 0)
                MessageUtilities.sendMessage("None");
            else
                MessageUtilities.sendMessage($"[FFFFFF]Use {GeneralUtilities.formatCmd("!settings default <setting>")} to reset a setting to its default.");
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
